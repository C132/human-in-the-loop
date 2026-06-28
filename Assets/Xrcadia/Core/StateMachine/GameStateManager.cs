using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xrcadia.Core.Services;
using Xrcadia.Core.Transitions;
using Debug = UnityEngine.Debug;

namespace Xrcadia.Core.StateMachine
{
    /// <summary>
    /// Authoritative finite-state machine (XRC-87). Plain C# so it can be driven by a
    /// headless test; a thin MonoBehaviour runner in the App layer pumps <see cref="Tick"/>
    /// and keeps it alive across scene loads.
    ///
    /// Stack model: exactly one base state plus zero or more overlays above it. Base states
    /// replace; overlays push/pop. Only the top-of-stack state is ticked; the state beneath
    /// an overlay receives <see cref="IGameState.OnPause"/>/<see cref="IGameState.OnResume"/>.
    /// </summary>
    public sealed class GameStateManager : IGameStateMachine
    {
        private readonly Dictionary<GameState, IGameState> _states = new Dictionary<GameState, IGameState>();
        private readonly List<IGameState> _overlays = new List<IGameState>();
        private readonly TransitionTable _table;
        private readonly StateContext _context;

        private IGameState _base;

        /// <summary>
        /// Minimum time the Loading overlay stays up during <see cref="TransitionTo"/> to
        /// avoid a flicker on fast work. Seconds.
        /// </summary>
        public float MinLoadingDwellSeconds { get; set; } = 0.5f;

        /// <summary>
        /// Async delay used for min-dwell. Injectable so the runtime can resume on the Unity
        /// main thread (via Awaitable) and tests can make it instant.
        /// </summary>
        public Func<float, Task> DelaySeconds { get; set; } = _ => Task.CompletedTask;

        public GameStateManager(ServiceRegistry services, LoadingProgress loading, TransitionTable table = null)
        {
            _table = table ?? TransitionTable.BuildDefault();
            _context = new StateContext(this, services, loading);
        }

        /// <summary>The shared context handed to states and (for binding) the UI router/screens.</summary>
        public StateContext Context => _context;

        public GameState CurrentBaseState => _base?.Id ?? GameState.None;

        public GameState CurrentState => _overlays.Count > 0 ? _overlays[_overlays.Count - 1].Id : CurrentBaseState;

        public bool Transitioning { get; private set; }

        public event Action<GameState, GameState> StateChanged;
        public event Action<StateChange> Transitioned;

        public void Register(IGameState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            _states[state.Id] = state;
        }

        /// <summary>Enter the initial Boot state. Cold-boot entry point.</summary>
        public Task Start() => GoTo(GameState.Boot);

        /// <summary>Pump the active (top-of-stack) state.</summary>
        public void Tick(float deltaTime)
        {
            if (_overlays.Count > 0)
            {
                _overlays[_overlays.Count - 1].Tick(deltaTime);
            }
            else
            {
                _base?.Tick(deltaTime);
            }
        }

        // -------- Public navigation API (guarded against re-entrancy) --------

        public async Task GoTo(GameState target)
        {
            if (!BeginTransition()) return;
            try
            {
                await ChangeBase(target);
            }
            finally
            {
                EndTransition();
            }
        }

        public async Task Fire(GameTrigger trigger)
        {
            // Let the active state resolve context-dependent triggers first (XRC-87:
            // conditional routing stays in the owning state, not the UI).
            if (TopState() is ITriggerHandler handler && handler.TryHandleTrigger(trigger))
            {
                return;
            }

            var from = CurrentBaseState;
            if (!_table.TryResolve(from, trigger, out var target))
            {
                Debug.LogWarning($"[FSM] Trigger {trigger} is not valid from {from}; ignored.");
                return;
            }

            await GoTo(target);
        }

        public async Task PushOverlay(GameState overlay)
        {
            if (!BeginTransition()) return;
            try
            {
                await PushOverlayInternal(overlay);
            }
            finally
            {
                EndTransition();
            }
        }

        public async Task PopOverlay()
        {
            if (!BeginTransition()) return;
            try
            {
                await PopOverlayInternal();
            }
            finally
            {
                EndTransition();
            }
        }

        public async Task TransitionTo(GameState target, Func<ITransitionReporter, Task> work)
        {
            if (!BeginTransition())
            {
                Debug.LogWarning($"[FSM] TransitionTo({target}) ignored — a transition is already in flight.");
                return;
            }

            try
            {
                _context.Loading.Reset();
                await PushOverlayInternal(GameState.Loading);

                var stopwatch = Stopwatch.StartNew();
                try
                {
                    if (work != null)
                    {
                        await work(_context.Loading);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[FSM] Transition work to {target} failed: {ex}");
                    await RouteToErrorOrRecover();
                    return;
                }

                // Minimum dwell so the overlay never flashes on fast work.
                var remaining = MinLoadingDwellSeconds - (float)stopwatch.Elapsed.TotalSeconds;
                if (remaining > 0f)
                {
                    await DelaySeconds(remaining);
                }

                await PopOverlayInternal();
                await ChangeBase(target);
            }
            finally
            {
                EndTransition();
            }
        }

        // -------- Internals (no guard; callers hold it) --------

        private async Task ChangeBase(GameState target)
        {
            var from = CurrentBaseState;

            if (!_table.IsAllowed(from, target))
            {
                Debug.LogError($"[FSM] Illegal transition {from} -> {target}; rejected.");
                return;
            }

            if (!_states.TryGetValue(target, out var next))
            {
                Debug.LogError($"[FSM] No state registered for {target}; rejected.");
                return;
            }

            // A base change clears any open overlays first.
            while (_overlays.Count > 0)
            {
                await PopOverlayInternal();
            }

            if (_base != null)
            {
                await _base.Exit();
            }

            _base = next;
            await _base.Enter(_context);

            Emit(new StateChange(from, target, StateChangeKind.Replace));
        }

        private async Task PushOverlayInternal(GameState overlay)
        {
            if (!overlay.IsOverlay())
            {
                Debug.LogError($"[FSM] {overlay} is not an overlay; use GoTo instead.");
                return;
            }

            if (!_states.TryGetValue(overlay, out var state))
            {
                Debug.LogError($"[FSM] No overlay registered for {overlay}; rejected.");
                return;
            }

            var from = CurrentState;
            TopState()?.OnPause();
            _overlays.Add(state);
            await state.Enter(_context);

            Emit(new StateChange(from, overlay, StateChangeKind.Push));
        }

        private async Task PopOverlayInternal()
        {
            if (_overlays.Count == 0)
            {
                return;
            }

            var top = _overlays[_overlays.Count - 1];
            var from = top.Id;
            await top.Exit();
            _overlays.RemoveAt(_overlays.Count - 1);

            var restored = CurrentState;
            TopState()?.OnResume();

            Emit(new StateChange(from, restored, StateChangeKind.Pop));
        }

        private async Task RouteToErrorOrRecover()
        {
            // Minimal error path for this vertical (full recovery is XRC-99). Ensure we never
            // leave the Loading overlay stuck up, then surface an error overlay if available.
            while (_overlays.Count > 0)
            {
                await PopOverlayInternal();
            }

            if (_states.ContainsKey(GameState.ErrorModal))
            {
                await PushOverlayInternal(GameState.ErrorModal);
            }
        }

        private IGameState TopState()
            => _overlays.Count > 0 ? _overlays[_overlays.Count - 1] : _base;

        private void Emit(StateChange change)
        {
            Transitioned?.Invoke(change);
            StateChanged?.Invoke(change.From, change.To);
        }

        private bool BeginTransition()
        {
            if (Transitioning)
            {
                return false;
            }

            Transitioning = true;
            return true;
        }

        private void EndTransition() => Transitioning = false;
    }
}

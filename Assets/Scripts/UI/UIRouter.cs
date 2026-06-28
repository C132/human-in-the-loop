using System.Collections.Generic;
using Xrcadia.Core.StateMachine;
using Debug = UnityEngine.Debug;

namespace Xrcadia.UI
{
    /// <summary>
    /// Maps FSM states to world-space screens (XRC-86). Subscribes to
    /// <see cref="IGameStateMachine.Transitioned"/> and shows/hides the matching screen or
    /// manages the overlay stack — no per-screen manual wiring. Overlays render above the
    /// base screen in stack order.
    /// </summary>
    public sealed class UIRouter
    {
        readonly IUIHost _host;
        readonly Dictionary<GameState, ScreenBase> _screens = new Dictionary<GameState, ScreenBase>();
        readonly List<ScreenBase> _shownOverlays = new List<ScreenBase>();

        ScreenBase _currentBase;
        IGameStateMachine _machine;

        public UIRouter(IUIHost host)
        {
            _host = host;
        }

        /// <summary>Register a screen and parent its (hidden) subtree under the host root.</summary>
        public void Register(ScreenBase screen, StateContext context)
        {
            screen.Initialize(context);
            _host.Root.Add(screen.Root);
            _screens[screen.State] = screen;
        }

        /// <summary>Begin reacting to FSM transitions.</summary>
        public void Bind(IGameStateMachine machine)
        {
            _machine = machine;
            _machine.Transitioned += OnTransitioned;
        }

        public void Unbind()
        {
            if (_machine != null)
            {
                _machine.Transitioned -= OnTransitioned;
                _machine = null;
            }
        }

        void OnTransitioned(StateChange change)
        {
            switch (change.Kind)
            {
                case StateChangeKind.Replace:
                    SwapBase(change.To);
                    break;
                case StateChangeKind.Push:
                    PushOverlay(change.To);
                    break;
                case StateChangeKind.Pop:
                    PopOverlay(change.From);
                    break;
            }
        }

        void SwapBase(GameState target)
        {
            _currentBase?.Hide();

            if (_screens.TryGetValue(target, out var next))
            {
                _currentBase = next;
                next.Show();
            }
            else
            {
                // States without a screen (e.g. Boot, Shutdown) are valid — nothing to draw.
                _currentBase = null;
            }
        }

        void PushOverlay(GameState overlay)
        {
            if (!_screens.TryGetValue(overlay, out var screen))
            {
                Debug.LogWarning($"[UIRouter] No screen registered for overlay {overlay}.");
                return;
            }

            screen.Show(); // BringToFront in Show keeps stack order correct.
            _shownOverlays.Add(screen);
        }

        void PopOverlay(GameState overlay)
        {
            if (_screens.TryGetValue(overlay, out var screen))
            {
                screen.Hide();
                _shownOverlays.Remove(screen);
            }
        }
    }
}

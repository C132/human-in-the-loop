using System;
using System.Threading.Tasks;
using Xrcadia.Core.Transitions;

namespace Xrcadia.Core.StateMachine
{
    /// <summary>
    /// The surface area states and screens use to drive navigation. No consumer loads
    /// scenes directly — every move goes through here (XRC-88 / XRC-89 requirement).
    /// </summary>
    public interface IGameStateMachine
    {
        /// <summary>The current base (non-overlay) state.</summary>
        GameState CurrentBaseState { get; }

        /// <summary>Top of the stack — an overlay if one is open, otherwise the base state.</summary>
        GameState CurrentState { get; }

        /// <summary>True while a transition is mid-flight (used to reject re-entrant requests).</summary>
        bool Transitioning { get; }

        /// <summary>Simple from/to event for audio/analytics consumers.</summary>
        event Action<GameState, GameState> StateChanged;

        /// <summary>Rich event (push/pop/replace) for the UI router.</summary>
        event Action<StateChange> Transitioned;

        /// <summary>Validated base-state change. Invalid edges are rejected and logged.</summary>
        Task GoTo(GameState target);

        /// <summary>Resolve a trigger against the transition table and move accordingly.</summary>
        Task Fire(GameTrigger trigger);

        /// <summary>
        /// Push the Loading overlay, await <paramref name="work"/>, pop, then enter
        /// <paramref name="target"/>. On failure routes to the error path rather than
        /// soft-locking. Re-entrant calls while transitioning are ignored.
        /// </summary>
        Task TransitionTo(GameState target, Func<ITransitionReporter, Task> work);

        Task PushOverlay(GameState overlay);

        Task PopOverlay();
    }
}

namespace Xrcadia.Core.StateMachine
{
    /// <summary>
    /// Implemented by states that need to resolve a trigger with runtime context the static
    /// <see cref="TransitionTable"/> can't express — e.g. Title routing to Onboarding on
    /// first launch vs MainMenu otherwise, or Main Menu kicking off async Continue/New Game
    /// work behind the Loading overlay. Keeps that logic in the owning state rather than the
    /// UI layer. The machine asks the active state first, then falls back to the table.
    /// </summary>
    public interface ITriggerHandler
    {
        /// <summary>
        /// Return true if this state consumed the trigger (and started handling it).
        /// Returning false lets the machine resolve it via the transition table.
        /// </summary>
        bool TryHandleTrigger(GameTrigger trigger);
    }
}

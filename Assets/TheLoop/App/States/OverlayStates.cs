using Xrcadia.Core.StateMachine;

namespace TheLoop.App.States
{
    /// <summary>
    /// Loading overlay state. Behaviorless — the visuals/progress live in the Loading screen
    /// bound to <c>StateContext.Loading</c>; the transition service drives the progress model.
    /// (Full transition system is XRC-98.)
    /// </summary>
    public sealed class LoadingOverlayState : GameStateBase
    {
        public override GameState Id => GameState.Loading;
    }

    /// <summary>
    /// Settings overlay state (XRC-91 slice). Behaviorless host for the Settings screen; the
    /// screen pops the overlay on Back.
    /// </summary>
    public sealed class SettingsOverlayState : GameStateBase
    {
        public override GameState Id => GameState.Settings;
    }
}

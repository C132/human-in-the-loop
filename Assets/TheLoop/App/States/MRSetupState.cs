using Xrcadia.Core.StateMachine;

namespace TheLoop.App.States
{
    /// <summary>
    /// The gate between the Hub and a Session (XRC-94). Validating the play space / passthrough
    /// and confirming the tabletop anchor is the screen's job (<see cref="Screens.MRSetupScreen"/>
    /// drives <see cref="MRSetupFlow"/>); this state owns the routes out: Begin → Session,
    /// Cancel → Hub. Loss of tracking mid-setup is surfaced through the recoverable error path
    /// (XRC-99) via <c>Machine.RaiseError</c>, so it never crashes.
    /// </summary>
    public sealed class MRSetupState : GameStateBase, ITriggerHandler
    {
        public override GameState Id => GameState.MRSetup;

        public bool TryHandleTrigger(GameTrigger trigger)
        {
            switch (trigger)
            {
                case GameTrigger.BeginSession:
                    Context.Machine.GoTo(GameState.Session).Forget();
                    return true;

                case GameTrigger.Cancel:
                    Context.Machine.GoTo(GameState.Hub).Forget();
                    return true;

                default:
                    return false;
            }
        }
    }
}

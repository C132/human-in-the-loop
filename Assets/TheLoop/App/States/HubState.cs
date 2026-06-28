using Xrcadia.Core.StateMachine;

namespace TheLoop.App.States
{
    /// <summary>
    /// The between-run home base (XRC-93). Navigation shell only — economy/upgrade depth is
    /// XRC-83. Settings pushes an overlay (so it restores the Hub on pop); Launch Run and Exit
    /// to Menu fall through to the transition table (Hub → MRSetup / Hub → MainMenu). "Launch"
    /// routes to MR Setup, never straight into a Session.
    /// </summary>
    public sealed class HubState : GameStateBase, ITriggerHandler
    {
        public override GameState Id => GameState.Hub;

        public bool TryHandleTrigger(GameTrigger trigger)
        {
            switch (trigger)
            {
                case GameTrigger.OpenSettings:
                    Context.Machine.PushOverlay(GameState.Settings).Forget();
                    return true;

                default:
                    // LaunchRun → MRSetup and ExitToMenu → MainMenu resolve via the table.
                    return false;
            }
        }
    }
}

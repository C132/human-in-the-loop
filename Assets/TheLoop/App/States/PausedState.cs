using Xrcadia.Core.StateMachine;

namespace TheLoop.App.States
{
    /// <summary>
    /// Pause overlay over an active Session (XRC-96). Pushing it stops the Session being ticked
    /// and fires its OnPause, so the run freezes; popping resumes the exact sub-state. Resume
    /// (pop) and Settings (a further overlay) are driven by the screen; Abandon is handled here:
    /// it records the run's partial outcome via the Session outcome path, then routes to the Hub.
    /// </summary>
    public sealed class PausedState : GameStateBase, ITriggerHandler
    {
        public override GameState Id => GameState.Paused;

        public bool TryHandleTrigger(GameTrigger trigger)
        {
            if (trigger != GameTrigger.Abandon)
            {
                return false;
            }

            // Hand back partial learnings (a non-terminal run builds a Failure outcome with the
            // tallies so far), then leave the run for the Hub.
            var run = Context.Run.Active;
            if (run != null)
            {
                Context.Run.LastOutcome = run.BuildOutcome();
                Context.Run.Active = null;
            }

            Context.Machine.GoTo(GameState.Hub).Forget();
            return true;
        }
    }
}

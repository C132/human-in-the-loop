using System.Threading.Tasks;
using Xrcadia.Core.StateMachine;

namespace TheLoop.App.States
{
    /// <summary>
    /// Post-run debrief (XRC-97). On entry it consumes the Session outcome, computes the payout
    /// (<see cref="RunPayout"/>), commits it to the save profile, and publishes the debrief for
    /// the screen. Clearing the consumed outcome guards against a double-commit. The single exit
    /// is back to the Hub (driven by the screen).
    /// </summary>
    public sealed class ResultsState : GameStateBase
    {
        public override GameState Id => GameState.Results;

        public override Task Enter(StateContext context)
        {
            base.Enter(context);

            var outcome = context.Run.LastOutcome;
            if (outcome == null)
            {
                return Task.CompletedTask; // nothing to debrief (e.g. re-entry)
            }

            var debrief = RunPayout.Compute(outcome);

            var profile = context.Save.Profile;
            if (profile != null)
            {
                profile.currency += debrief.CurrencyAwarded;
                profile.agentXp += debrief.XpAwarded;
                profile.runsCompleted += 1;
                context.Save.Save(); // commit payouts before returning
            }

            context.Run.LastDebrief = debrief;
            context.Run.LastOutcome = null; // consumed
            return Task.CompletedTask;
        }
    }
}

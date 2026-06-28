using Xrcadia.Core.StateMachine;

namespace TheLoop.App
{
    /// <summary>
    /// Turns a run outcome into a debrief + payout (XRC-97). A deliberately small, deterministic
    /// rule for the prototype — the real economy (upgrade trees, learning model) is XRC-83. Kept
    /// pure so the payout is unit-testable; the Results state applies it to the save profile.
    /// </summary>
    public static class RunPayout
    {
        private const int SuccessCurrencyBonus = 10;
        private const int SuccessXpBonus = 5;
        private const int FailureXpConsolation = 1;

        public static RunDebrief Compute(RunOutcome outcome)
        {
            var success = outcome.Result == RunResult.Success;

            return new RunDebrief
            {
                Result = outcome.Result,
                CurrencyAwarded = outcome.Score + (success ? SuccessCurrencyBonus : 0),
                XpAwarded = outcome.PlacedTiles + (success ? SuccessXpBonus : FailureXpConsolation),
                LearningRecap = success
                    ? "The agent reached the goal — it reinforced the path you shaped."
                    : "The agent went down — it learned which routes fail.",
            };
        }
    }
}

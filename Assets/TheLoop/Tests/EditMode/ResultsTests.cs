using System.Threading.Tasks;
using NUnit.Framework;
using TheLoop.App;
using TheLoop.App.States;
using Xrcadia.Core.Services;
using Xrcadia.Core.StateMachine;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-97 Results / debrief: the payout rule, and that a finished run commits the right
    /// payout to the save and always returns to the Hub with the save persisted.
    /// </summary>
    public sealed class ResultsTests
    {
        private static async Task DriveToSession(TestHarness h, SessionState session)
        {
            h.Machine.Register(new MRSetupState());
            h.Machine.Register(session);
            h.Machine.Register(new ResultsState());
            await Flow.DriveToHub(h);
            await h.Machine.Fire(GameTrigger.LaunchRun);
            await h.Machine.Fire(GameTrigger.BeginSession);
            await Flow.WaitUntilIdle(h);
        }

        [Test]
        public void Payout_Success_AddsBonus_Failure_Consoles()
        {
            var win = RunPayout.Compute(new RunOutcome { Result = RunResult.Success, Score = 5, PlacedTiles = 2 });
            Assert.AreEqual(5 + 10, win.CurrencyAwarded);
            Assert.AreEqual(2 + 5, win.XpAwarded);
            Assert.AreEqual(RunResult.Success, win.Result);

            var loss = RunPayout.Compute(new RunOutcome { Result = RunResult.Failure, Score = 5, PlacedTiles = 2 });
            Assert.AreEqual(5, loss.CurrencyAwarded, "no success bonus on failure");
            Assert.AreEqual(2 + 1, loss.XpAwarded, "a small consolation xp on failure");
        }

        [Test]
        public async Task SuccessfulRun_CommitsPayout_AndReturnsToHub_SavePersisted()
        {
            var h = new TestHarness(onboarded: true);
            var session = new SessionState();
            await DriveToSession(h, session);

            var profile = h.Save.Profile;
            var runsBefore = profile.runsCompleted;
            var currencyBefore = profile.currency;

            session.Run.WorldReady();
            session.Run.AdvanceAgent();
            session.Run.Context.Score = 7;
            session.Run.Context.PlacedTiles = 3;
            session.Run.ReachGoal();
            session.Run.Settle();                  // Session hands off to Results, which commits
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Results, h.Machine.CurrentBaseState);
            Assert.AreEqual(runsBefore + 1, profile.runsCompleted, "the run is tallied");
            Assert.AreEqual(currencyBefore + 7 + 10, profile.currency, "score + success bonus committed");
            Assert.AreEqual(RunResult.Success, h.Machine.Context.Run.LastDebrief.Result);

            // Single exit: return to Hub, with the save persisted to the store.
            await h.Machine.GoTo(GameState.Hub);
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Hub, h.Machine.CurrentBaseState);

            var reloaded = new SaveService(h.Store);
            await reloaded.Load();
            Assert.AreEqual(runsBefore + 1, reloaded.Profile.runsCompleted, "the payout is persisted");
        }

        [Test]
        public async Task FailedRun_CommitsFailurePayout_AndDebriefsFailure()
        {
            var h = new TestHarness(onboarded: true);
            var session = new SessionState();
            await DriveToSession(h, session);

            var currencyBefore = h.Save.Profile.currency;

            session.Run.WorldReady();
            session.Run.AdvanceAgent();
            session.Run.Context.Score = 4;
            session.Run.AgentDown();
            session.Run.Settle();
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Results, h.Machine.CurrentBaseState);
            Assert.AreEqual(RunResult.Failure, h.Machine.Context.Run.LastDebrief.Result);
            Assert.AreEqual(currencyBefore + 4, h.Save.Profile.currency, "failure pays the score, no bonus");
        }
    }
}

using System.Threading.Tasks;
using NUnit.Framework;
using TheLoop.App.States;
using Xrcadia.Core.StateMachine;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-95 Session controller: a completed run exits to Results with the correct outcome,
    /// pause/resume preserves the sub-state, and tracking loss mid-run is handled.
    /// </summary>
    public sealed class SessionTests
    {
        private sealed class PlaceholderState : GameStateBase
        {
            private readonly GameState _id;
            public PlaceholderState(GameState id) => _id = id;
            public override GameState Id => _id;
        }

        private static async Task DriveToSession(TestHarness h, SessionState session)
        {
            h.Machine.Register(new MRSetupState());
            h.Machine.Register(session);
            await Flow.DriveToHub(h);
            await h.Machine.Fire(GameTrigger.LaunchRun);    // Hub -> MRSetup
            await h.Machine.Fire(GameTrigger.BeginSession);  // MRSetup -> Session
            await Flow.WaitUntilIdle(h);
        }

        [Test]
        public async Task CompletedRun_ExitsToResults_WithSuccessOutcome()
        {
            var h = new TestHarness(onboarded: true);
            h.Machine.Register(new PlaceholderState(GameState.Results));
            var session = new SessionState();
            await DriveToSession(h, session);
            Assert.AreEqual(GameState.Session, h.Machine.CurrentBaseState);

            var run = session.Run;
            run.WorldReady();
            run.AdvanceAgent();
            run.ReachGoal();
            run.Settle();                       // terminal -> Session hands off to Results
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Results, h.Machine.CurrentBaseState, "a completed run exits to Results");
            Assert.IsNotNull(h.Machine.Context.Run.LastOutcome, "an outcome is handed to Results");
            Assert.AreEqual(RunResult.Success, h.Machine.Context.Run.LastOutcome.Result);
        }

        [Test]
        public async Task FailedRun_HandsFailureOutcomeToResults()
        {
            var h = new TestHarness(onboarded: true);
            h.Machine.Register(new PlaceholderState(GameState.Results));
            var session = new SessionState();
            await DriveToSession(h, session);

            var run = session.Run;
            run.WorldReady();
            run.AdvanceAgent();
            run.AgentDown();
            run.Settle();
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Results, h.Machine.CurrentBaseState);
            Assert.AreEqual(RunResult.Failure, h.Machine.Context.Run.LastOutcome.Result);
        }

        [Test]
        public async Task PauseAndResume_PreservesSubState()
        {
            var h = new TestHarness(onboarded: true);
            var session = new SessionState();
            h.Machine.Register(new PlaceholderState(GameState.Paused)); // real overlay is XRC-96
            await DriveToSession(h, session);

            session.Run.WorldReady();
            session.Run.AdvanceAgent();
            Assert.AreEqual(RunPhase.AgentRunning, session.Run.Phase);

            await h.Machine.PushOverlay(GameState.Paused);
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Paused, h.Machine.CurrentState);
            Assert.AreEqual(GameState.Session, h.Machine.CurrentBaseState);

            await h.Machine.PopOverlay();
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Session, h.Machine.CurrentState);
            Assert.AreEqual(RunPhase.AgentRunning, session.Run.Phase, "the sub-state survives pause/resume");
        }

        [Test]
        public async Task TrackingLoss_IsHandled_AndResumesTheRun()
        {
            var h = new TestHarness(onboarded: true);
            var session = new SessionState();
            h.Machine.Register(new ErrorModalState());
            await DriveToSession(h, session);

            session.Run.WorldReady();
            session.Run.AdvanceAgent();

            await h.Machine.RaiseError(new GameError
            {
                Severity = ErrorSeverity.Recoverable,
                Title = "Tracking lost",
                Message = "Restore tracking to continue the run.",
                Source = "Tracking",
            });
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.ErrorModal, h.Machine.CurrentState, "tracking loss is handled");
            Assert.AreEqual(GameState.Session, h.Machine.CurrentBaseState);

            await h.Machine.ResumeFromError();
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Session, h.Machine.CurrentState, "resumes the run");
            Assert.AreEqual(RunPhase.AgentRunning, session.Run.Phase, "sub-state intact through the error");
        }
    }
}

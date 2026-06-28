using System.Threading.Tasks;
using NUnit.Framework;
using TheLoop.App.States;
using Xrcadia.Core.StateMachine;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-96 Pause overlay: pausing freezes the run and resumes it identically, Settings stacks
    /// above Pause and restores it, and Abandon records partial learnings then lands in the Hub.
    /// </summary>
    public sealed class PauseTests
    {
        private static async Task DriveToSession(TestHarness h, SessionState session)
        {
            h.Machine.Register(new MRSetupState());
            h.Machine.Register(session);
            h.Machine.Register(new PausedState());
            await Flow.DriveToHub(h);
            await h.Machine.Fire(GameTrigger.LaunchRun);
            await h.Machine.Fire(GameTrigger.BeginSession);
            await Flow.WaitUntilIdle(h);
        }

        [Test]
        public async Task PauseAndResume_FreezesAndRestoresRunIdentically()
        {
            var h = new TestHarness(onboarded: true);
            var session = new SessionState();
            await DriveToSession(h, session);

            session.Run.WorldReady();
            session.Run.AdvanceAgent();
            Assert.AreEqual(RunPhase.AgentRunning, session.Run.Phase);

            await h.Machine.PushOverlay(GameState.Paused);
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Paused, h.Machine.CurrentState);
            Assert.AreEqual(GameState.Session, h.Machine.CurrentBaseState, "the run is frozen beneath the pause");

            await h.Machine.PopOverlay(); // Resume
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Session, h.Machine.CurrentState);
            Assert.AreEqual(RunPhase.AgentRunning, session.Run.Phase, "the run resumes identically");
        }

        [Test]
        public async Task Settings_StacksAbovePause_AndRestoresIt()
        {
            var h = new TestHarness(onboarded: true);
            var session = new SessionState();
            await DriveToSession(h, session);
            h.Machine.Register(new SettingsOverlayState());

            await h.Machine.PushOverlay(GameState.Paused);
            await h.Machine.PushOverlay(GameState.Settings);
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Settings, h.Machine.CurrentState, "Settings stacks above the pause");
            Assert.AreEqual(GameState.Session, h.Machine.CurrentBaseState);

            await h.Machine.PopOverlay(); // close Settings
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Paused, h.Machine.CurrentState, "closing Settings restores the pause");

            await h.Machine.PopOverlay(); // resume
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Session, h.Machine.CurrentState);
        }

        [Test]
        public async Task Abandon_RecordsPartialOutcome_AndLandsInHub()
        {
            var h = new TestHarness(onboarded: true);
            var session = new SessionState();
            await DriveToSession(h, session);

            session.Run.WorldReady();
            session.Run.AdvanceAgent(); // some progress to abandon

            await h.Machine.PushOverlay(GameState.Paused);
            await Flow.WaitUntilIdle(h);

            await h.Machine.Fire(GameTrigger.Abandon); // confirmed in the screen, fires the trigger
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Hub, h.Machine.CurrentBaseState, "abandon lands in the Hub");
            Assert.IsNotNull(h.Machine.Context.Run.LastOutcome, "partial learnings recorded via the outcome path");
            Assert.IsNull(h.Machine.Context.Run.Active, "the run is no longer active");
        }
    }
}

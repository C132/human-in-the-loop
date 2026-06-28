using System.Threading.Tasks;
using NUnit.Framework;
using TheLoop.App;
using TheLoop.App.States;
using Xrcadia.Core.Services;
using Xrcadia.Core.StateMachine;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-94 MR Setup gate: the check → guidance/confirm flow, the routes out (Begin → Session,
    /// Cancel → Hub), and tracking loss mid-setup routing to the recoverable error path.
    /// </summary>
    public sealed class MRSetupTests
    {
        private sealed class PlaceholderState : GameStateBase
        {
            private readonly GameState _id;
            public PlaceholderState(GameState id) => _id = id;
            public override GameState Id => _id;
        }

        private static async Task DriveToMRSetup(TestHarness h)
        {
            await Flow.DriveToHub(h);
            await h.Machine.Fire(GameTrigger.LaunchRun);
            await Flow.WaitUntilIdle(h);
        }

        // --- Flow logic (unit) ---

        [Test]
        public void Flow_InvalidSpace_GuidesThenReadyConfirms()
        {
            var flow = new MRSetupFlow();
            Assert.AreEqual(MRSetupFlow.Phase.Checking, flow.Current);
            Assert.IsFalse(flow.CanBegin);

            flow.Apply(new MRSpaceStatus(tracking: true, passthrough: false, surface: true));
            Assert.AreEqual(MRSetupFlow.Phase.Guidance, flow.Current, "a missing condition guides setup");
            Assert.IsFalse(flow.CanBegin);

            flow.BeginCheck();
            Assert.AreEqual(MRSetupFlow.Phase.Checking, flow.Current, "re-check returns to checking");

            flow.Apply(MRSpaceStatus.Ready);
            Assert.AreEqual(MRSetupFlow.Phase.ConfirmAnchor, flow.Current, "a ready space confirms the anchor");
            Assert.IsTrue(flow.CanBegin);
        }

        [Test]
        public void SpaceStatus_IsReady_OnlyWhenAllConditionsMet()
        {
            Assert.IsTrue(MRSpaceStatus.Ready.IsReady);
            Assert.IsFalse(new MRSpaceStatus(true, true, false).IsReady);
            Assert.IsFalse(new MRSpaceStatus(false, true, true).IsReady);
        }

        [Test]
        public async Task Service_DefaultsToReady_AndHonorsInjectedCheck()
        {
            Assert.IsTrue((await new MRService().CheckSpace()).IsReady, "no checker = ready (editor/headless)");

            var mr = new MRService(() => Task.FromResult(new MRSpaceStatus(false, true, true)));
            Assert.IsFalse((await mr.CheckSpace()).IsReady);
        }

        // --- FSM routing ---

        [Test]
        public async Task Begin_RoutesToSession()
        {
            var h = new TestHarness(onboarded: true);
            h.Machine.Register(new MRSetupState());
            h.Machine.Register(new PlaceholderState(GameState.Session));
            await DriveToMRSetup(h);
            Assert.AreEqual(GameState.MRSetup, h.Machine.CurrentBaseState);

            await h.Machine.Fire(GameTrigger.BeginSession);
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Session, h.Machine.CurrentBaseState);
        }

        [Test]
        public async Task Cancel_ReturnsToHub()
        {
            var h = new TestHarness(onboarded: true);
            h.Machine.Register(new MRSetupState());
            await DriveToMRSetup(h);

            await h.Machine.Fire(GameTrigger.Cancel);
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Hub, h.Machine.CurrentBaseState);
        }

        [Test]
        public async Task TrackingLossDuringSetup_IsHandled_AndResumes()
        {
            var h = new TestHarness(onboarded: true);
            h.Machine.Register(new MRSetupState());
            h.Machine.Register(new ErrorModalState());
            await DriveToMRSetup(h);

            await h.Machine.RaiseError(new GameError
            {
                Severity = ErrorSeverity.Recoverable,
                Title = "Tracking lost",
                Message = "Restore tracking.",
                Source = "Tracking",
            });
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.ErrorModal, h.Machine.CurrentState, "tracking loss is handled, not a crash");
            Assert.AreEqual(GameState.MRSetup, h.Machine.CurrentBaseState, "setup preserved beneath the overlay");

            await h.Machine.ResumeFromError();
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.MRSetup, h.Machine.CurrentState, "resumes setup cleanly");
        }
    }
}

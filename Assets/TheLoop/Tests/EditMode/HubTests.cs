using System.Threading.Tasks;
using NUnit.Framework;
using Xrcadia.Core.StateMachine;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-93 Hub / Lab shell routing. Reachability from Main Menu (Continue/New) is covered by
    /// <see cref="LoadingFlowTests"/>; the full Hub → run → Hub spine by
    /// <see cref="FsmFullPathTests"/>. Covered here: the Hub's own routes out — Launch → MR
    /// Setup, Settings overlay restoring the Hub, Exit → Main Menu — and that the profile
    /// survives the run round trip.
    /// </summary>
    public sealed class HubTests
    {
        private sealed class PlaceholderState : GameStateBase
        {
            private readonly GameState _id;
            public PlaceholderState(GameState id) => _id = id;
            public override GameState Id => _id;
        }

        [Test]
        public async Task LaunchRun_RoutesToMRSetup_NotSession()
        {
            var h = new TestHarness(onboarded: true);
            h.Machine.Register(new PlaceholderState(GameState.MRSetup)); // real state is XRC-94
            await Flow.DriveToHub(h);

            await h.Machine.Fire(GameTrigger.LaunchRun);
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.MRSetup, h.Machine.CurrentBaseState, "Launch routes to MR Setup");
        }

        [Test]
        public async Task Settings_OverlaysHub_AndRestoresOnPop()
        {
            var h = new TestHarness(onboarded: true);
            await Flow.DriveToHub(h);

            await h.Machine.Fire(GameTrigger.OpenSettings);
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Settings, h.Machine.CurrentState, "Settings overlays the Hub");
            Assert.AreEqual(GameState.Hub, h.Machine.CurrentBaseState, "Hub stays the base beneath the overlay");

            await h.Machine.PopOverlay();
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Hub, h.Machine.CurrentState, "popping Settings restores the Hub");
        }

        [Test]
        public async Task ExitToMenu_ReturnsToMainMenu()
        {
            var h = new TestHarness(onboarded: true);
            await Flow.DriveToHub(h);

            await h.Machine.Fire(GameTrigger.ExitToMenu);
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentBaseState);
        }

        [Test]
        public async Task SurvivesRunRoundTrip_WithProfileIntact()
        {
            var h = new TestHarness(onboarded: true);
            h.Machine.Register(new PlaceholderState(GameState.MRSetup));
            h.Machine.Register(new PlaceholderState(GameState.Session));
            h.Machine.Register(new PlaceholderState(GameState.Results));
            await Flow.DriveToHub(h);

            var profile = h.Save.Profile; // created by New Game on the way into the Hub
            Assert.IsNotNull(profile);

            // Hub → MRSetup → Session → Results → Hub.
            await h.Machine.Fire(GameTrigger.LaunchRun);
            await h.Machine.GoTo(GameState.Session);
            await h.Machine.GoTo(GameState.Results);
            await h.Machine.GoTo(GameState.Hub);
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Hub, h.Machine.CurrentBaseState, "round trip lands back in the Hub");
            Assert.AreSame(profile, h.Save.Profile, "the same profile is intact after the round trip");
        }
    }
}

using System.Threading.Tasks;
using NUnit.Framework;
using Xrcadia.Core.StateMachine;

namespace Xrcadia.Tests.EditMode
{
    /// <summary>
    /// XRC-88 acceptance: cold boot reaches an interactive Main Menu with no manual steps;
    /// Continue reflects save state; Quit tears down; onboarding shows once.
    /// </summary>
    public sealed class LoadingFlowTests
    {
        [Test]
        public async Task ColdBoot_InitializesServicesBehindLoading_ThenReachesSplash()
        {
            var h = new TestHarness(onboarded: true);

            await h.Machine.Start();
            Assert.AreEqual(GameState.Boot, h.Machine.CurrentBaseState);

            await h.Machine.TransitionTo(GameState.Splash, h.BootWork);

            Assert.IsTrue(h.Spy.Initialized, "core services initialize during boot");
            Assert.AreEqual(GameState.Splash, h.Machine.CurrentBaseState);

            // The Loading overlay was pushed and popped around the async boot work.
            var pushedLoading = h.Changes.FindIndex(c =>
                c.Kind == StateChangeKind.Push && c.To == GameState.Loading);
            var poppedLoading = h.Changes.FindIndex(c =>
                c.Kind == StateChangeKind.Pop && c.From == GameState.Loading);
            var enteredSplash = h.Changes.FindIndex(c =>
                c.Kind == StateChangeKind.Replace && c.To == GameState.Splash);

            Assert.Greater(pushedLoading, -1, "Loading overlay should be pushed");
            Assert.Greater(poppedLoading, pushedLoading, "Loading overlay should be popped after push");
            Assert.Greater(enteredSplash, poppedLoading, "Splash entered after Loading pops");
        }

        [Test]
        public async Task ColdBoot_ReachesInteractiveMainMenu_NoManualSteps()
        {
            var h = new TestHarness(onboarded: true);
            await Flow.DriveToMainMenu(h);

            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentBaseState);
        }

        [Test]
        public async Task FirstLaunch_ShowsOnboardingOnce_ThenSkips()
        {
            // First launch: Title -> Onboarding -> MainMenu.
            var first = new TestHarness(onboarded: false);
            await first.Machine.Start();
            await first.Machine.TransitionTo(GameState.Splash, first.BootWork);
            await first.Machine.GoTo(GameState.Title);

            Assert.IsTrue(first.Settings.IsFirstLaunch);
            await first.Machine.Fire(GameTrigger.StartPressed);
            Assert.AreEqual(GameState.Onboarding, first.Machine.CurrentBaseState);
            Assert.IsFalse(first.Settings.IsFirstLaunch, "onboarding marks itself complete on entry");

            await first.Machine.Fire(GameTrigger.OnboardingComplete);
            Assert.AreEqual(GameState.MainMenu, first.Machine.CurrentBaseState);
            Assert.IsTrue(first.Visited(GameState.Onboarding));

            // Subsequent launch (flag persisted): Title -> MainMenu, no onboarding.
            var second = new TestHarness(onboarded: true);
            await second.Machine.Start();
            await second.Machine.TransitionTo(GameState.Splash, second.BootWork);
            await second.Machine.GoTo(GameState.Title);
            await second.Machine.Fire(GameTrigger.StartPressed);

            Assert.AreEqual(GameState.MainMenu, second.Machine.CurrentBaseState);
            Assert.IsFalse(second.Visited(GameState.Onboarding), "onboarding never shows again");
        }

        [Test]
        public async Task Continue_DisabledWithoutSave()
        {
            var h = new TestHarness(onboarded: true, hasSave: false);
            await Flow.DriveToMainMenu(h);

            Assert.IsFalse(h.Save.HasSave);

            // Firing Continue with no save is consumed but does nothing.
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning,
                new System.Text.RegularExpressions.Regex("Continue fired with no save"));
            await h.Machine.Fire(GameTrigger.Continue);
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentBaseState);
        }

        [Test]
        public async Task Continue_EnabledWithSave_EntersHub()
        {
            var h = new TestHarness(onboarded: true, hasSave: true);
            await Flow.DriveToMainMenu(h);

            Assert.IsTrue(h.Save.HasSave);
            await h.Machine.Fire(GameTrigger.Continue);
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Hub, h.Machine.CurrentBaseState);
        }

        [Test]
        public async Task NewGame_CreatesSave_EntersHub()
        {
            var h = new TestHarness(onboarded: true, hasSave: false);
            await Flow.DriveToMainMenu(h);

            await h.Machine.Fire(GameTrigger.NewGame);
            await Flow.WaitUntilIdle(h);

            Assert.IsTrue(h.Save.HasSave, "New Game creates a save");
            Assert.AreEqual(GameState.Hub, h.Machine.CurrentBaseState);
        }

        [Test]
        public async Task Quit_TearsDownServices_AndEntersShutdown()
        {
            var h = new TestHarness(onboarded: true);
            await Flow.DriveToMainMenu(h);

            await h.Machine.Fire(GameTrigger.Quit);
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Shutdown, h.Machine.CurrentBaseState);
            Assert.IsTrue(h.Spy.ShutDown, "services are shut down on quit");
            Assert.IsTrue(h.QuitCalled, "application quit is requested");
        }
    }
}

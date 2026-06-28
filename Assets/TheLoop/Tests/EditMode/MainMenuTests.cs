using System.Threading.Tasks;
using NUnit.Framework;
using Xrcadia.Core.StateMachine;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-89 acceptance for the Main Menu route. Drives the menu through
    /// <see cref="GameStateManager.Fire"/> — the same surface the screen buttons use — so the
    /// <c>MainMenuState</c> trigger wiring is exercised, not just the underlying machine.
    ///
    /// The Continue (with/without save), New Game and Quit→Shutdown routes are asserted in
    /// <see cref="LoadingFlowTests"/>; this fixture owns the Settings option, whose trigger
    /// path (Settings button → OpenSettings → overlay push) had no direct coverage.
    /// </summary>
    public sealed class MainMenuTests
    {
        [Test]
        public async Task SettingsOption_OpensOverlay_AndRestoresMainMenuIntact()
        {
            var h = new TestHarness(onboarded: true);
            await Flow.DriveToMainMenu(h);

            // Settings option → overlay above an intact Main Menu base.
            await h.Machine.Fire(GameTrigger.OpenSettings);
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Settings, h.Machine.CurrentState, "Settings is the active overlay");
            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentBaseState, "Main Menu stays the base beneath the overlay");

            // Back (PopOverlay) restores the Main Menu as the active state.
            await h.Machine.PopOverlay();
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentState, "popping Settings restores the Main Menu");
            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentBaseState);
        }

        [Test]
        public async Task SettingsOption_PushesOverlay_WithoutReplacingBase()
        {
            // The requirement is explicit: Settings opens as an overlay (push), not a base edge.
            // Guards against a regression that re-wires it as a MainMenu → Settings replace.
            var h = new TestHarness(onboarded: true);
            await Flow.DriveToMainMenu(h);

            await h.Machine.Fire(GameTrigger.OpenSettings);
            await Flow.WaitUntilIdle(h);

            Assert.IsTrue(
                h.Changes.Exists(c => c.Kind == StateChangeKind.Push && c.To == GameState.Settings),
                "Settings is pushed as an overlay");
            Assert.IsFalse(
                h.Changes.Exists(c => c.Kind == StateChangeKind.Replace && c.To == GameState.Settings),
                "Settings must never replace the base state");
        }
    }
}

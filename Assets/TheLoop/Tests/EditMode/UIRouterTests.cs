using System;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-86 acceptance (headless slice). The in-headset world-space rendering criterion is
    /// inherently manual, but the router contract the whole framework leans on is testable:
    /// changing game state swaps the visible screen with no per-screen wiring, and overlay
    /// pushes render above the base screen and restore it on pop. Driven through a fake
    /// <see cref="IUIHost"/> — the seam the interface exists to provide — and stub screens.
    /// </summary>
    public sealed class UIRouterTests
    {
        /// <summary>An in-memory host: a real panel root, no PanelRenderer / MonoBehaviour.</summary>
        private sealed class FakeUIHost : IUIHost
        {
            public VisualElement Root { get; } = new VisualElement();
            public event Action<VisualElement> RootChanged { add { } remove { } }
        }

        /// <summary>A behaviorless screen that only reports whether the router is showing it.</summary>
        private sealed class StubScreen : ScreenBase
        {
            private readonly GameState _state;
            public StubScreen(GameState state) => _state = state;
            public override GameState State => _state;
            protected override VisualElement Build() => new VisualElement();
            public bool Visible => Root.style.display.value == DisplayStyle.Flex;
        }

        [Test]
        public async Task Router_SwapsVisibleScreen_OnStateChange_WithNoPerScreenWiring()
        {
            var h = new TestHarness(onboarded: true);
            var router = new UIRouter(new FakeUIHost());

            var loading = new StubScreen(GameState.Loading); // shown/hidden during boot transitions
            var title = new StubScreen(GameState.Title);
            var mainMenu = new StubScreen(GameState.MainMenu);
            router.Register(loading, h.Machine.Context);
            router.Register(title, h.Machine.Context);
            router.Register(mainMenu, h.Machine.Context);
            router.Bind(h.Machine);

            // The screens never reference the router; the router maps state → screen purely
            // off the machine's transition events.
            await Flow.DriveToMainMenu(h);

            Assert.IsTrue(mainMenu.Visible, "router shows the screen for the active base state");
            Assert.IsFalse(title.Visible, "the previous base screen is hidden");
        }

        [Test]
        public async Task Router_OverlayRendersAboveBase_AndRestoresOnPop()
        {
            var h = new TestHarness(onboarded: true);
            var router = new UIRouter(new FakeUIHost());

            var loading = new StubScreen(GameState.Loading);
            var mainMenu = new StubScreen(GameState.MainMenu);
            var settings = new StubScreen(GameState.Settings);
            router.Register(loading, h.Machine.Context);
            router.Register(mainMenu, h.Machine.Context);
            router.Register(settings, h.Machine.Context);
            router.Bind(h.Machine);

            await Flow.DriveToMainMenu(h);
            Assert.IsTrue(mainMenu.Visible);
            Assert.IsFalse(settings.Visible);

            await h.Machine.PushOverlay(GameState.Settings);
            await Flow.WaitUntilIdle(h);
            Assert.IsTrue(settings.Visible, "overlay screen is shown on push");
            Assert.IsTrue(mainMenu.Visible, "base screen stays visible beneath the overlay");

            await h.Machine.PopOverlay();
            await Flow.WaitUntilIdle(h);
            Assert.IsFalse(settings.Visible, "overlay screen hides on pop");
            Assert.IsTrue(mainMenu.Visible, "base screen is restored as the visible screen");
        }
    }
}

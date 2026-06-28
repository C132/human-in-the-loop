using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>Placeholder first-run screen. Real content is XRC-90.</summary>
    public sealed class OnboardingScreen : ScreenBase
    {
        public override GameState State => GameState.Onboarding;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Heading("Welcome, Researcher"));
            panel.Add(Ui.Subtitle("Onboarding content coming soon (XRC-90)."));
            panel.Add(Ui.MenuButton("Continue",
                () => Context.Machine.Fire(GameTrigger.OnboardingComplete).Forget()));

            root.Add(panel);
            return root;
        }
    }
}

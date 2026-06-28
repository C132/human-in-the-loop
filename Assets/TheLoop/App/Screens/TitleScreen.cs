using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    public sealed class TitleScreen : ScreenBase
    {
        public override GameState State => GameState.Title;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Eyebrow("XRCADIA · MIXED-REALITY STRATEGY ROGUELIKE"));
            panel.Add(Ui.Title("Human in the Loop"));
            panel.Add(Ui.Subtitle("You don't play the hero. You train the one that does."));

            var bar = Ui.ButtonBar();
            bar.Add(Ui.PrimaryButton("Look / Press to Start",
                () => Context.Machine.Fire(GameTrigger.StartPressed).Forget()));
            panel.Add(bar);

            root.Add(panel);
            return root;
        }
    }
}

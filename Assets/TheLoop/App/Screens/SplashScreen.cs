using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace Xrcadia.App.Screens
{
    public sealed class SplashScreen : ScreenBase
    {
        public override GameState State => GameState.Splash;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Title("XRCADIA"));
            panel.Add(Ui.Subtitle("Human in the Loop"));
            panel.Add(Ui.Prompt("Look or press to skip"));
            root.Add(panel);

            // Any click/poke on the splash requests a skip (gated by min dwell in the state).
            root.RegisterCallback<ClickEvent>(_ => Context.Machine.Fire(GameTrigger.StartPressed).Forget());
            return root;
        }
    }
}

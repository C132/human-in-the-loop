using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace Xrcadia.App.Screens
{
    /// <summary>Placeholder Hub screen (real shell is XRC-93). Confirms the transition and routes back.</summary>
    public sealed class HubScreen : ScreenBase
    {
        public override GameState State => GameState.Hub;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Heading("Hub / Lab"));
            panel.Add(Ui.Subtitle("Coming soon (XRC-93)."));
            panel.Add(Ui.MenuButton("Exit to Menu", () => Context.Machine.GoTo(GameState.MainMenu).Forget()));

            root.Add(panel);
            return root;
        }
    }
}

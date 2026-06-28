using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// Pause overlay screen (XRC-96). Resume pops back to the frozen run; Settings stacks a
    /// further overlay above the pause (and restores it on close); Abandon confirms inline, then
    /// fires the Abandon trigger which records partial learnings and routes to the Hub.
    /// </summary>
    public sealed class PauseScreen : ScreenBase
    {
        private VisualElement _menu;
        private VisualElement _confirm;

        public override GameState State => GameState.Paused;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Heading("Paused"));

            _menu = new VisualElement();
            _menu.Add(Ui.MenuButton("Resume", () => Context.Machine.PopOverlay().Forget()));
            _menu.Add(Ui.MenuButton("Settings", () => Context.Machine.PushOverlay(GameState.Settings).Forget()));
            _menu.Add(Ui.MenuButton("Abandon Run", ShowConfirm));
            panel.Add(_menu);

            _confirm = new VisualElement();
            _confirm.Add(Ui.Subtitle("Abandon the run and return to the Hub? Partial progress is kept."));
            _confirm.Add(Ui.MenuButton("Abandon", () => Context.Machine.Fire(GameTrigger.Abandon).Forget()));
            _confirm.Add(Ui.MenuButton("Keep Playing", ShowMenu));
            panel.Add(_confirm);

            root.Add(panel);
            return root;
        }

        public override void Bind() => ShowMenu();

        private void ShowMenu()
        {
            _menu.style.display = DisplayStyle.Flex;
            _confirm.style.display = DisplayStyle.None;
        }

        private void ShowConfirm()
        {
            _menu.style.display = DisplayStyle.None;
            _confirm.style.display = DisplayStyle.Flex;
        }
    }
}

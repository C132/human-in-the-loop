using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// Main Menu screen (XRC-89 / XRC-92). Continue is enabled only when a save exists; every
    /// option fires a trigger, never a direct load. New Game gates behind an inline overwrite
    /// confirmation when a save already exists — confirming fires New Game (which overwrites),
    /// cancelling returns to the menu.
    /// </summary>
    public sealed class MainMenuScreen : ScreenBase
    {
        private VisualElement _menu;
        private VisualElement _confirm;
        private Button _continueButton;

        public override GameState State => GameState.MainMenu;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Eyebrow("MIXED-REALITY STRATEGY ROGUELIKE"));
            panel.Add(Ui.Title("Human in the Loop"));

            // --- Main menu options ---
            _menu = new VisualElement();
            _continueButton = Ui.MenuButton("Continue",
                () => Context.Machine.Fire(GameTrigger.Continue).Forget());
            _menu.Add(_continueButton);
            _menu.Add(Ui.MenuButton("New Game", OnNewGame));
            _menu.Add(Ui.MenuButton("Settings",
                () => Context.Machine.Fire(GameTrigger.OpenSettings).Forget()));
            _menu.Add(Ui.MenuButton("Quit",
                () => Context.Machine.Fire(GameTrigger.Quit).Forget()));
            panel.Add(_menu);

            // --- Overwrite confirmation (hidden until New Game with an existing save) ---
            _confirm = new VisualElement();
            _confirm.Add(Ui.Subtitle("Overwrite your existing save and start a new game?"));
            _confirm.Add(Ui.MenuButton("Overwrite",
                () => Context.Machine.Fire(GameTrigger.NewGame).Forget()));
            _confirm.Add(Ui.MenuButton("Cancel", ShowMenu));
            panel.Add(_confirm);

            root.Add(panel);
            return root;
        }

        public override void Bind()
        {
            // Reflect save state every time the menu is shown (e.g. after New Game returns).
            _continueButton.SetEnabled(Context.Save.HasSave);
            ShowMenu();
        }

        private void OnNewGame()
        {
            if (Context.Save.HasSave)
                ShowConfirm();
            else
                Context.Machine.Fire(GameTrigger.NewGame).Forget();
        }

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

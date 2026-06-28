using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// Main Menu screen (XRC-89 slice). Continue is enabled only when a save exists; every
    /// option fires a trigger, never a direct load.
    /// </summary>
    public sealed class MainMenuScreen : ScreenBase
    {
        private Button _continueButton;

        public override GameState State => GameState.MainMenu;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Title("Human in the Loop"));

            _continueButton = Ui.MenuButton("Continue",
                () => Context.Machine.Fire(GameTrigger.Continue).Forget());
            panel.Add(_continueButton);

            panel.Add(Ui.MenuButton("New Game",
                () => Context.Machine.Fire(GameTrigger.NewGame).Forget()));
            panel.Add(Ui.MenuButton("Settings",
                () => Context.Machine.Fire(GameTrigger.OpenSettings).Forget()));
            panel.Add(Ui.MenuButton("Quit",
                () => Context.Machine.Fire(GameTrigger.Quit).Forget()));

            root.Add(panel);
            return root;
        }

        public override void Bind()
        {
            // Reflect save state every time the menu is shown (e.g. after New Game returns).
            _continueButton.SetEnabled(Context.Save.HasSave);
        }
    }
}

using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// Fatal error screen (XRC-99). The unrecoverable landing — corrupt save, unsupported data.
    /// Presents the failure calmly and a single defined exit: Return to Main Menu.
    /// </summary>
    public sealed class FatalErrorScreen : ScreenBase
    {
        private Label _message;

        public override GameState State => GameState.Fatal;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Heading("Something broke"));
            _message = Ui.Subtitle(string.Empty);
            panel.Add(_message);

            panel.Add(Ui.MenuButton("Return to Main Menu",
                () => Context.Machine.SafeExitToMainMenu().Forget()));

            root.Add(panel);
            return root;
        }

        public override void Bind()
        {
            var error = Context.Error.Current;
            _message.text = error?.Message ?? "An unrecoverable error occurred. Your save is unchanged.";
        }
    }
}

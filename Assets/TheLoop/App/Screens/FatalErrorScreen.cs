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

            var badge = new Label("UNRECOVERABLE");
            badge.AddToClassList("badge");
            badge.AddToClassList("badge--danger");
            panel.Add(badge);

            panel.Add(Ui.Heading("Something broke"));
            _message = Ui.Subtitle(string.Empty);
            panel.Add(_message);

            var bar = Ui.ButtonBar();
            bar.Add(Ui.PrimaryButton("Return to Main Menu",
                () => Context.Machine.SafeExitToMainMenu().Forget()));
            panel.Add(bar);

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

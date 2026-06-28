using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// Recoverable error overlay screen (XRC-99). Shows calm guidance for the active error
    /// (tracking loss, passthrough drop, transient load failure) and two routes: Resume — pop
    /// back to the preserved state once resolved — or Safe Exit to the Main Menu.
    /// </summary>
    public sealed class ErrorModalScreen : ScreenBase
    {
        private Label _title;
        private Label _message;

        public override GameState State => GameState.ErrorModal;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            _title = Ui.Heading("Something went wrong");
            _message = Ui.Subtitle(string.Empty);
            panel.Add(_title);
            panel.Add(_message);

            panel.Add(Ui.MenuButton("Resume", () => Context.Machine.ResumeFromError().Forget()));
            panel.Add(Ui.MenuButton("Safe Exit", () => Context.Machine.SafeExitToMainMenu().Forget()));

            root.Add(panel);
            return root;
        }

        public override void Bind()
        {
            var error = Context.Error.Current;
            _title.text = string.IsNullOrEmpty(error?.Title) ? "Something went wrong" : error.Title;
            _message.text = error?.Message ?? "Try to resolve the issue, then resume.";
        }
    }
}

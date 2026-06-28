using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// First-run onboarding screen (XRC-90). Walks the placeholder teaching beats in
    /// <see cref="OnboardingContent"/> with Back/Next, and a Skip that finishes immediately.
    /// Both finishing the last step and skipping fire <see cref="GameTrigger.OnboardingComplete"/>
    /// → Main Menu; the once-only flag is set by <see cref="States.OnboardingState"/> on entry,
    /// so skipping still counts as onboarded.
    /// </summary>
    public sealed class OnboardingScreen : ScreenBase
    {
        private readonly OnboardingSequence _sequence = new OnboardingSequence(OnboardingContent.Steps.Count);

        private Label _heading;
        private Label _body;
        private Label _progress;
        private Button _back;
        private Button _next;

        public override GameState State => GameState.Onboarding;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            _heading = Ui.Heading(string.Empty);
            _body = Ui.Subtitle(string.Empty);
            _progress = Ui.Prompt(string.Empty);
            panel.Add(_heading);
            panel.Add(_body);
            panel.Add(_progress);

            _back = Ui.MenuButton("Back", OnBack);
            _next = Ui.MenuButton("Next", OnNext);
            panel.Add(_back);
            panel.Add(_next);

            // Skip still sets the onboarded flag (set on state entry), so it never reappears.
            panel.Add(Ui.MenuButton("Skip", Complete));

            root.Add(panel);
            return root;
        }

        public override void Bind()
        {
            _sequence.Reset();
            RenderStep();
        }

        private void OnBack()
        {
            _sequence.Back();
            RenderStep();
        }

        private void OnNext()
        {
            if (_sequence.IsLast)
            {
                Complete();
                return;
            }

            _sequence.Next();
            RenderStep();
        }

        private void Complete() => Context.Machine.Fire(GameTrigger.OnboardingComplete).Forget();

        private void RenderStep()
        {
            var step = OnboardingContent.Steps[_sequence.Index];
            _heading.text = step.Title;
            _body.text = step.Body;
            _progress.text = $"{_sequence.Index + 1} / {OnboardingContent.Steps.Count}";
            _back.SetEnabled(!_sequence.IsFirst);
            _next.text = _sequence.IsLast ? "Enter the Lab" : "Next";
        }
    }
}

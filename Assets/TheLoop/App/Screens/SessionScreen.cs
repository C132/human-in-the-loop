using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// Minimal Session screen (XRC-95). Reflects the run sub-FSM phase published on
    /// <c>StateContext.Run</c>; the actual tabletop gameplay rendering is owned by the core loop
    /// (XRC-78). Subscribes to phase changes while shown so the label tracks the run live.
    /// </summary>
    public sealed class SessionScreen : ScreenBase
    {
        private Label _phase;
        private RunSubMachine _subscribed;

        public override GameState State => GameState.Session;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Heading("Session"));
            _phase = Ui.Subtitle(string.Empty);
            panel.Add(_phase);
            panel.Add(Ui.Prompt("The agent runs the loop you shape — gameplay lands with XRC-78."));

            root.Add(panel);
            return root;
        }

        public override void Bind() => Render(Context.Run.Active?.Phase);

        protected override void OnShow()
        {
            _subscribed = Context.Run.Active;
            if (_subscribed != null)
            {
                _subscribed.PhaseChanged += OnPhase;
            }
        }

        protected override void OnHide()
        {
            if (_subscribed != null)
            {
                _subscribed.PhaseChanged -= OnPhase;
                _subscribed = null;
            }
        }

        private void OnPhase(RunPhase phase) => Render(phase);

        private void Render(RunPhase? phase) => _phase.text = phase == null ? string.Empty : $"Phase: {phase}";
    }
}

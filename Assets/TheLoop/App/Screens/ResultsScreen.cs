using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// Results / debrief screen (XRC-97). Presents the run outcome, the payout the Results state
    /// committed, and a readable "what the agent learned" recap (the transparent-agent pillar).
    /// Single exit: Return to Hub.
    /// </summary>
    public sealed class ResultsScreen : ScreenBase
    {
        private Label _outcome;
        private Label _rewards;
        private Label _recap;

        public override GameState State => GameState.Results;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Heading("Debrief"));
            _outcome = Ui.Subtitle(string.Empty);
            _rewards = Ui.Prompt(string.Empty);
            _recap = Ui.Prompt(string.Empty);
            panel.Add(_outcome);
            panel.Add(_rewards);
            panel.Add(_recap);

            panel.Add(Ui.MenuButton("Return to Hub", () => Context.Machine.GoTo(GameState.Hub).Forget()));

            root.Add(panel);
            return root;
        }

        public override void Bind()
        {
            var debrief = Context.Run.LastDebrief;
            if (debrief == null)
            {
                _outcome.text = "Run complete.";
                _rewards.text = string.Empty;
                _recap.text = string.Empty;
                return;
            }

            _outcome.text = debrief.Result == RunResult.Success ? "Run successful" : "Run failed";
            _rewards.text = $"+{debrief.CurrencyAwarded} credits · +{debrief.XpAwarded} agent XP";
            _recap.text = debrief.LearningRecap;
        }
    }
}

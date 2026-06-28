using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// Results / debrief screen (XRC-97 / XRC-100). A colored outcome badge, the committed
    /// payout as a stat block, and a readable "what the agent learned" recap. Single exit: Hub.
    /// </summary>
    public sealed class ResultsScreen : ScreenBase
    {
        private Label _badge;
        private Label _credits;
        private Label _xp;
        private Label _recap;

        public override GameState State => GameState.Results;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.AddToClassList("panel--wide");
            panel.Add(Ui.Eyebrow("RUN DEBRIEF"));
            panel.Add(Ui.Heading("Debrief"));

            _badge = new Label();
            _badge.AddToClassList("badge");
            panel.Add(_badge);

            var rewards = Ui.Section("REWARDS");
            _credits = Stat();
            _xp = Stat();
            rewards.Add(Ui.Row("Credits earned", _credits));
            rewards.Add(Ui.Row("Agent XP", _xp));
            panel.Add(rewards);

            var learned = Ui.Section("WHAT THE AGENT LEARNED");
            _recap = Ui.Body(string.Empty);
            learned.Add(_recap);
            panel.Add(learned);

            var bar = Ui.ButtonBar();
            bar.Add(Ui.PrimaryButton("Return to Hub", () => Context.Machine.GoTo(GameState.Hub).Forget()));
            panel.Add(bar);

            root.Add(panel);
            return root;
        }

        public override void Bind()
        {
            _badge.RemoveFromClassList("badge--success");
            _badge.RemoveFromClassList("badge--danger");

            var d = Context.Run.LastDebrief;
            if (d == null)
            {
                _badge.text = "RUN COMPLETE";
                _credits.text = "—";
                _xp.text = "—";
                _recap.text = string.Empty;
                return;
            }

            var ok = d.Result == RunResult.Success;
            _badge.text = ok ? "RUN SUCCESSFUL" : "RUN FAILED";
            _badge.AddToClassList(ok ? "badge--success" : "badge--danger");
            _credits.text = "+" + d.CurrencyAwarded;
            _xp.text = "+" + d.XpAwarded;
            _recap.text = d.LearningRecap;
        }

        private static Label Stat()
        {
            var l = new Label();
            l.AddToClassList("stat");
            return l;
        }
    }
}

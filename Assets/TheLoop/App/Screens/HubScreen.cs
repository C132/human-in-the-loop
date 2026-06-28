using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// Hub / Lab shell screen (XRC-93 / XRC-100). A STATUS stat block read from the save profile,
    /// then the routes out: Launch Run → MR Setup (primary), Settings, Exit to Menu. Every action
    /// fires a trigger — no direct scene loads. Refine-agent / select-seed depth is XRC-83.
    /// </summary>
    public sealed class HubScreen : ScreenBase
    {
        private Label _lab;
        private Label _runs;
        private Label _credits;

        public override GameState State => GameState.Hub;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.AddToClassList("panel--wide");
            panel.Add(Ui.Eyebrow("THE LAB"));
            panel.Add(Ui.Heading("Hub"));

            var status = Ui.Section("STATUS");
            _lab = Stat();
            _runs = Stat();
            _credits = Stat();
            status.Add(Ui.Row("Lab level", _lab));
            status.Add(Ui.Row("Runs completed", _runs));
            status.Add(Ui.Row("Credits", _credits));
            panel.Add(status);

            var bar = Ui.ButtonBar();
            bar.Add(Ui.PrimaryButton("Launch Run",
                () => Context.Machine.Fire(GameTrigger.LaunchRun).Forget()));
            bar.Add(Ui.MenuButton("Settings",
                () => Context.Machine.Fire(GameTrigger.OpenSettings).Forget()));
            bar.Add(Ui.GhostButton("Exit to Menu",
                () => Context.Machine.Fire(GameTrigger.ExitToMenu).Forget()));
            panel.Add(bar);

            panel.Add(Ui.Caption("Review run · Refine agent · Select seed — coming soon."));

            root.Add(panel);
            return root;
        }

        public override void Bind()
        {
            var p = Context.Save.Profile;
            _lab.text = p == null ? "—" : p.labLevel.ToString();
            _runs.text = p == null ? "—" : p.runsCompleted.ToString();
            _credits.text = p == null ? "—" : p.currency.ToString();
        }

        private static Label Stat()
        {
            var l = new Label();
            l.AddToClassList("stat");
            return l;
        }
    }
}

using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// Hub / Lab shell screen (XRC-93). Surfaces the between-run actions and the routes out:
    /// Launch Run → MR Setup, Settings overlay, Exit to Menu. Reads the meta-progression
    /// summary from the save profile (refine-agent / select-seed depth is XRC-83). Every action
    /// fires a trigger — no direct scene loads.
    /// </summary>
    public sealed class HubScreen : ScreenBase
    {
        private Label _summary;

        public override GameState State => GameState.Hub;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Heading("Hub / Lab"));

            _summary = Ui.Subtitle(string.Empty);
            panel.Add(_summary);

            panel.Add(Ui.MenuButton("Launch Run",
                () => Context.Machine.Fire(GameTrigger.LaunchRun).Forget()));
            panel.Add(Ui.MenuButton("Settings",
                () => Context.Machine.Fire(GameTrigger.OpenSettings).Forget()));
            panel.Add(Ui.MenuButton("Exit to Menu",
                () => Context.Machine.Fire(GameTrigger.ExitToMenu).Forget()));

            // Depth (review debrief, refine agent, select seed) lands with XRC-83.
            panel.Add(Ui.Prompt("Review run · Refine agent · Select seed — coming soon."));

            root.Add(panel);
            return root;
        }

        public override void Bind()
        {
            var p = Context.Save.Profile;
            _summary.text = p == null
                ? "New lab"
                : $"Lab level {p.labLevel} · {p.runsCompleted} runs · {p.currency} credits";
        }
    }
}

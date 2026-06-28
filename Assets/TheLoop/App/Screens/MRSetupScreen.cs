using System.Threading.Tasks;
using UnityEngine.UIElements;
using Xrcadia.Core.Services;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// MR Setup screen (XRC-94). Checks the play space via the MR service, then either guides
    /// the player to fix it (re-check loop) or confirms the tabletop anchor. The primary button
    /// is Begin when ready or Re-check otherwise; Cancel returns to the Hub.
    /// </summary>
    public sealed class MRSetupScreen : ScreenBase
    {
        private readonly MRSetupFlow _flow = new MRSetupFlow();

        private Label _status;
        private Button _primary;

        public override GameState State => GameState.MRSetup;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Heading("MR Setup"));
            _status = Ui.Subtitle(string.Empty);
            panel.Add(_status);

            _primary = Ui.MenuButton("Re-check", OnPrimary);
            panel.Add(_primary);
            panel.Add(Ui.MenuButton("Cancel", () => Context.Machine.Fire(GameTrigger.Cancel).Forget()));

            root.Add(panel);
            return root;
        }

        public override void Bind() => Recheck();

        private void OnPrimary()
        {
            if (_flow.CanBegin)
                Context.Machine.Fire(GameTrigger.BeginSession).Forget();
            else
                Recheck();
        }

        private void Recheck()
        {
            _flow.BeginCheck();
            Render();
            CheckAsync().Forget();
        }

        private async Task CheckAsync()
        {
            _flow.Apply(await Context.MR.CheckSpace());
            Render();
        }

        private void Render()
        {
            switch (_flow.Current)
            {
                case MRSetupFlow.Phase.Checking:
                    _status.text = "Checking your play space…";
                    _primary.text = "Re-check";
                    _primary.SetEnabled(false);
                    break;
                case MRSetupFlow.Phase.ConfirmAnchor:
                    _status.text = "Space ready. Confirm your table anchor, then begin.";
                    _primary.text = "Begin";
                    _primary.SetEnabled(true);
                    break;
                case MRSetupFlow.Phase.Guidance:
                    _status.text = Guidance(_flow.Status);
                    _primary.text = "Re-check";
                    _primary.SetEnabled(true);
                    break;
            }
        }

        private static string Guidance(MRSpaceStatus s)
        {
            if (!s.Tracking) return "Tracking lost — make sure your headset can see the room, then re-check.";
            if (!s.Passthrough) return "Turn on passthrough to place the tabletop, then re-check.";
            if (!s.Surface) return "Clear a flat surface and define your boundary, then re-check.";
            return "Set up your play space, then re-check.";
        }
    }
}

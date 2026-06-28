using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// Settings overlay screen (XRC-91 slice). Placeholder content with a single working
    /// control (master volume) to prove overlay push/pop and binding; Back pops the overlay
    /// and restores the Main Menu intact.
    /// </summary>
    public sealed class SettingsScreen : ScreenBase
    {
        private Slider _volume;

        public override GameState State => GameState.Settings;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Heading("Settings"));
            panel.Add(Ui.Subtitle("Full settings coming soon (XRC-91)."));

            _volume = new Slider("Master Volume", 0f, 1f);
            _volume.RegisterValueChangedCallback(evt => Context.Settings.SetMasterVolume(evt.newValue));
            panel.Add(_volume);

            panel.Add(Ui.MenuButton("Back", () => Context.Machine.PopOverlay().Forget()));

            root.Add(panel);
            return root;
        }

        public override void Bind()
        {
            _volume.SetValueWithoutNotify(Context.Settings.MasterVolume);
        }
    }
}

using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// Settings overlay screen (XRC-91). Placeholder categories — audio, comfort (MR) and
    /// accessibility — each wired through the settings service so changes persist across
    /// restarts. Back pops the overlay, restoring whatever base state it was opened from
    /// (Main Menu now; Hub and Pause once XRC-93 / XRC-96 land). Graphics, controls and save
    /// management are stubbed labels — their option lists are out of scope here.
    /// </summary>
    public sealed class SettingsScreen : ScreenBase
    {
        private Slider _master;
        private Slider _music;
        private Toggle _vignette;
        private Toggle _snapTurn;
        private Toggle _subtitles;

        public override GameState State => GameState.Settings;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Heading("Settings"));

            panel.Add(Ui.Subtitle("Audio"));
            _master = AddSlider(panel, "Master Volume", v => Context.Settings.SetMasterVolume(v));
            _music = AddSlider(panel, "Music Volume", v => Context.Settings.SetMusicVolume(v));

            panel.Add(Ui.Subtitle("Comfort"));
            _vignette = AddToggle(panel, "Comfort vignette", v => Context.Settings.SetComfortVignette(v));
            _snapTurn = AddToggle(panel, "Snap turning", v => Context.Settings.SetSnapTurn(v));

            panel.Add(Ui.Subtitle("Accessibility"));
            _subtitles = AddToggle(panel, "Subtitles", v => Context.Settings.SetSubtitles(v));

            // Owned elsewhere / tuning out of scope — shown so the category set reads complete.
            panel.Add(Ui.Prompt("Graphics · Controls · Save management — coming soon."));

            panel.Add(Ui.MenuButton("Back", () => Context.Machine.PopOverlay().Forget()));

            root.Add(panel);
            return root;
        }

        public override void Bind()
        {
            var s = Context.Settings;
            _master.SetValueWithoutNotify(s.MasterVolume);
            _music.SetValueWithoutNotify(s.MusicVolume);
            _vignette.SetValueWithoutNotify(s.ComfortVignette);
            _snapTurn.SetValueWithoutNotify(s.SnapTurn);
            _subtitles.SetValueWithoutNotify(s.Subtitles);
        }

        private static Slider AddSlider(VisualElement parent, string label, System.Action<float> onChange)
        {
            var slider = new Slider(label, 0f, 1f);
            slider.RegisterValueChangedCallback(evt => onChange(evt.newValue));
            parent.Add(slider);
            return slider;
        }

        private static Toggle AddToggle(VisualElement parent, string label, System.Action<bool> onChange)
        {
            var toggle = new Toggle(label);
            toggle.RegisterValueChangedCallback(evt => onChange(evt.newValue));
            parent.Add(toggle);
            return toggle;
        }
    }
}

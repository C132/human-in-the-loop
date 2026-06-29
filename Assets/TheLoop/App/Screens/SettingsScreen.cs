using System;
using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// Settings overlay screen (XRC-91 / XRC-100). Grouped, world-space sections — audio, comfort
    /// (MR) and accessibility — each wired through the settings service so changes persist across
    /// restarts. Back pops the overlay, restoring whatever base state it was opened from.
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
            panel.AddToClassList("panel--wide");
            panel.Add(Ui.Heading("Settings"));

            var audio = Ui.Section("AUDIO");
            _master = AddSlider(audio, "Master Volume", v => Context.Settings.SetMasterVolume(v));
            _music = AddSlider(audio, "Music Volume", v => Context.Settings.SetMusicVolume(v));
            panel.Add(audio);

            var comfort = Ui.Section("COMFORT");
            _vignette = AddToggle(comfort, "Comfort vignette", v => Context.Settings.SetComfortVignette(v));
            _snapTurn = AddToggle(comfort, "Snap turning", v => Context.Settings.SetSnapTurn(v));
            panel.Add(comfort);

            var access = Ui.Section("ACCESSIBILITY");
            _subtitles = AddToggle(access, "Subtitles", v => Context.Settings.SetSubtitles(v));
            panel.Add(access);

            panel.Add(Ui.Caption("Graphics · Controls · Save management — coming soon."));

            var bar = Ui.ButtonBar();
            bar.Add(Ui.GhostButton("Back", () => Context.Machine.PopOverlay().Forget()));
            panel.Add(bar);

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

        private static Slider AddSlider(VisualElement parent, string label, Action<float> onChange)
        {
            var slider = new Slider(label, 0f, 1f);
            slider.RegisterValueChangedCallback(evt => onChange(evt.newValue));
            parent.Add(slider);
            return slider;
        }

        private static Toggle AddToggle(VisualElement parent, string label, Action<bool> onChange)
        {
            var toggle = new Toggle(label);
            toggle.RegisterValueChangedCallback(evt => onChange(evt.newValue));
            parent.Add(toggle);
            return toggle;
        }
    }
}

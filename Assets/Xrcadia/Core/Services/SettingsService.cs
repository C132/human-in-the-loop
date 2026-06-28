using System.Threading.Tasks;

namespace Xrcadia.Core.Services
{
    /// <summary>
    /// App-level preferences, persisted through the key-value store and re-read on Boot
    /// (Initialize) so changes survive a restart. Holds the first-launch / onboarding flag
    /// that routes Title → Onboarding exactly once (XRC-88), plus the placeholder Settings
    /// preferences spanning audio / comfort / accessibility (XRC-91). The final option list
    /// and per-platform graphics tuning are out of scope; these are representative values that
    /// prove the read/write/persist path the Settings overlay binds to.
    /// </summary>
    public sealed class SettingsService : IAppService
    {
        private const string KeyOnboarded = "hitl.onboarding.completed";
        private const string KeyMasterVolume = "hitl.audio.master";
        private const string KeyMusicVolume = "hitl.audio.music";
        private const string KeyComfortVignette = "hitl.comfort.vignette";
        private const string KeySnapTurn = "hitl.comfort.snapturn";
        private const string KeySubtitles = "hitl.access.subtitles";

        private readonly IKeyValueStore _store;

        public SettingsService(IKeyValueStore store)
        {
            _store = store;
        }

        public string Name => "Settings";

        /// <summary>True on the very first launch (or until onboarding is marked complete).</summary>
        public bool FirstLaunch => _store.GetInt(KeyOnboarded, 0) == 0;

        // --- Audio ---
        public float MasterVolume { get; private set; } = 1f;
        public float MusicVolume { get; private set; } = 1f;

        // --- Comfort (MR) ---
        /// <summary>Vignette during locomotion to reduce motion discomfort. On by default.</summary>
        public bool ComfortVignette { get; private set; } = true;
        /// <summary>Snap turning rather than smooth — the comfort-first default.</summary>
        public bool SnapTurn { get; private set; } = true;

        // --- Accessibility ---
        public bool Subtitles { get; private set; } = false;

        public Task Initialize()
        {
            MasterVolume = ReadVolume(KeyMasterVolume);
            MusicVolume = ReadVolume(KeyMusicVolume);
            ComfortVignette = ReadBool(KeyComfortVignette, true);
            SnapTurn = ReadBool(KeySnapTurn, true);
            Subtitles = ReadBool(KeySubtitles, false);
            return Task.CompletedTask;
        }

        /// <summary>Persist that onboarding has been seen so it never shows again.</summary>
        public void MarkOnboardingComplete()
        {
            _store.SetInt(KeyOnboarded, 1);
            _store.Save();
        }

        public void SetMasterVolume(float value) => MasterVolume = WriteVolume(KeyMasterVolume, value);
        public void SetMusicVolume(float value) => MusicVolume = WriteVolume(KeyMusicVolume, value);
        public void SetComfortVignette(bool on) => ComfortVignette = WriteBool(KeyComfortVignette, on);
        public void SetSnapTurn(bool on) => SnapTurn = WriteBool(KeySnapTurn, on);
        public void SetSubtitles(bool on) => Subtitles = WriteBool(KeySubtitles, on);

        public void Shutdown() { }

        // Volumes are stored as 0..100 ints to keep the key-value store integer-only.
        private float ReadVolume(string key) => _store.GetInt(key, 100) / 100f;

        private float WriteVolume(string key, float value)
        {
            var clamped = value < 0f ? 0f : (value > 1f ? 1f : value);
            _store.SetInt(key, (int)(clamped * 100f));
            _store.Save();
            return clamped;
        }

        private bool ReadBool(string key, bool defaultValue) => _store.GetInt(key, defaultValue ? 1 : 0) == 1;

        private bool WriteBool(string key, bool value)
        {
            _store.SetInt(key, value ? 1 : 0);
            _store.Save();
            return value;
        }
    }
}

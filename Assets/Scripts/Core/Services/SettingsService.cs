using System.Threading.Tasks;

namespace Xrcadia.Core.Services
{
    /// <summary>
    /// App-level preferences. Owns the persisted first-launch / onboarding flag that
    /// routes Title → Onboarding exactly once (XRC-88). Settings content/UI is XRC-91;
    /// this only holds the few prototype values the loading flow needs.
    /// </summary>
    public sealed class SettingsService : IAppService
    {
        const string KeyOnboarded = "hitl.onboarding.completed";
        const string KeyMasterVolume = "hitl.audio.master";

        readonly IKeyValueStore _store;

        public SettingsService(IKeyValueStore store)
        {
            _store = store;
        }

        public string Name => "Settings";

        /// <summary>True on the very first launch (or until onboarding is marked complete).</summary>
        public bool IsFirstLaunch => _store.GetInt(KeyOnboarded, 0) == 0;

        public float MasterVolume { get; private set; } = 1f;

        public Task Initialize()
        {
            MasterVolume = ReadVolume();
            return Task.CompletedTask;
        }

        /// <summary>Persist that onboarding has been seen so it never shows again.</summary>
        public void MarkOnboardingComplete()
        {
            _store.SetInt(KeyOnboarded, 1);
            _store.Save();
        }

        public void SetMasterVolume(float value)
        {
            MasterVolume = value < 0f ? 0f : (value > 1f ? 1f : value);
            _store.SetInt(KeyMasterVolume, (int)(MasterVolume * 100f));
            _store.Save();
        }

        public void Shutdown() { }

        float ReadVolume() => _store.GetInt(KeyMasterVolume, 100) / 100f;
    }
}

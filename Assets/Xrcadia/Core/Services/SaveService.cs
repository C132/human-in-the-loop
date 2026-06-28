using System;
using System.Threading.Tasks;
using UnityEngine;
using Xrcadia.Core.StateMachine;
using Xrcadia.Core.Transitions;

namespace Xrcadia.Core.Services
{
    /// <summary>
    /// Minimal meta-progression profile (XRC-92). Schema is intentionally small for the
    /// prototype — the final progression model is owned by XRC-83. <see cref="version"/> exists
    /// so future migrations have a hook and so an out-of-range version reads as corruption.
    /// </summary>
    [Serializable]
    public sealed class SaveProfile
    {
        public int version;
        public long createdAtTicks;
        public long updatedAtTicks;
        public int labLevel;
        public int agentXp;
        public int runsCompleted;
        public int currency;
    }

    /// <summary>
    /// Thrown when a save cannot be read (unparseable or an unsupported version). A corrupt save
    /// is fatal (XRC-99) — it carries that severity so the transition service safe-exits to the
    /// Main Menu rather than looping the player back into a broken load.
    /// </summary>
    public sealed class SaveCorruptionException : FatalError
    {
        public SaveCorruptionException(string message, Exception inner = null)
            : base("Save", "Save data error", message, inner) { }
    }

    /// <summary>
    /// Save / profile service (XRC-92). Backs the Continue/New Game decision and persists
    /// meta-progression between sessions. A profile is stored as JSON in the key-value store;
    /// <see cref="HasSave"/> reports whether a record exists, while <see cref="Load"/> validates
    /// it and throws <see cref="SaveCorruptionException"/> on bad/old data so the caller's
    /// transition routes into recovery instead of crashing.
    /// </summary>
    public sealed class SaveService : IAppService
    {
        public const int CurrentVersion = 1;

        /// <summary>Key the profile JSON is stored under. Public so tools/tests can target it.</summary>
        public const string ProfileKey = "hitl.save.profile";

        private readonly IKeyValueStore _store;
        private SaveProfile _profile;

        public SaveService(IKeyValueStore store)
        {
            _store = store;
        }

        public string Name => "Save";

        /// <summary>True when a save record exists. Validity is only checked on <see cref="Load"/>.</summary>
        public bool HasSave => _store.HasKey(ProfileKey) && !string.IsNullOrEmpty(_store.GetString(ProfileKey));

        /// <summary>The active profile, or null until <see cref="Load"/> / <see cref="CreateNew"/> runs.</summary>
        public SaveProfile Profile => _profile;

        public Task Initialize() => Task.CompletedTask;

        /// <summary>Create a fresh profile (New Game). Overwrites any existing save.</summary>
        public Task CreateNew(ITransitionReporter reporter = null)
        {
            reporter?.Report(0.3f, "Creating profile…");

            var now = DateTime.UtcNow.Ticks;
            _profile = new SaveProfile
            {
                version = CurrentVersion,
                createdAtTicks = now,
                updatedAtTicks = now,
                labLevel = 1,
                agentXp = 0,
                runsCompleted = 0,
                currency = 0,
            };
            Persist();

            reporter?.Report(1f, "Profile ready");
            return Task.CompletedTask;
        }

        /// <summary>Load the existing profile (Continue). Throws on corrupt / unsupported data.</summary>
        public Task Load(ITransitionReporter reporter = null)
        {
            reporter?.Report(0.3f, "Loading save…");

            var json = _store.GetString(ProfileKey, string.Empty);
            if (string.IsNullOrEmpty(json))
                throw new SaveCorruptionException("No save data to load.");

            SaveProfile parsed;
            try
            {
                parsed = JsonUtility.FromJson<SaveProfile>(json);
            }
            catch (Exception ex)
            {
                throw new SaveCorruptionException("Save data could not be parsed.", ex);
            }

            if (parsed == null || parsed.version <= 0 || parsed.version > CurrentVersion)
                throw new SaveCorruptionException($"Unsupported save version: {parsed?.version ?? 0}.");

            _profile = parsed;
            reporter?.Report(1f, "Save loaded");
            return Task.CompletedTask;
        }

        /// <summary>Persist the active profile (e.g. after a run payout — XRC-97).</summary>
        public void Save()
        {
            if (_profile == null)
                return;

            _profile.updatedAtTicks = DateTime.UtcNow.Ticks;
            Persist();
        }

        public void Delete()
        {
            _store.DeleteKey(ProfileKey);
            _store.Save();
            _profile = null;
        }

        public void Shutdown() { }

        private void Persist()
        {
            _store.SetString(ProfileKey, JsonUtility.ToJson(_profile));
            _store.Save();
        }
    }
}

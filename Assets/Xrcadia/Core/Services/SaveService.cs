using System.Threading.Tasks;

namespace Xrcadia.Core.Services
{
    /// <summary>
    /// Stub save/profile service (XRC-88 scope). Backs the Continue/New Game gate via a
    /// persisted "save exists" flag. Real profile schema, versioning and corruption
    /// recovery land with XRC-92 — this only provides <see cref="HasSave"/> and the
    /// create/delete hooks the Main Menu needs.
    /// </summary>
    public sealed class SaveService : IAppService
    {
        private const string KeySaveExists = "hitl.save.exists";

        private readonly IKeyValueStore _store;

        public SaveService(IKeyValueStore store)
        {
            _store = store;
        }

        public string Name => "Save";

        /// <summary>True when a profile exists. Gates the Continue option.</summary>
        public bool HasSave => _store.GetInt(KeySaveExists, 0) == 1;

        public Task Initialize() => Task.CompletedTask;

        /// <summary>
        /// Create a fresh profile (New Game). Returns a Task so XRC-92 can make it genuinely
        /// async (real disk write) without changing call sites; the Loading overlay is kept up
        /// by the transition's minimum dwell regardless.
        /// </summary>
        public Task CreateNew(Transitions.ITransitionReporter reporter = null)
        {
            reporter?.Report(0.5f, "Creating profile…");
            _store.SetInt(KeySaveExists, 1);
            _store.Save();
            reporter?.Report(1f, "Profile ready");
            return Task.CompletedTask;
        }

        /// <summary>Load the existing profile (Continue). Stubbed for XRC-92.</summary>
        public Task Load(Transitions.ITransitionReporter reporter = null)
        {
            reporter?.Report(0.5f, "Loading save…");
            reporter?.Report(1f, "Save loaded");
            return Task.CompletedTask;
        }

        public void Delete()
        {
            _store.DeleteKey(KeySaveExists);
            _store.Save();
        }

        public void Shutdown() { }
    }
}

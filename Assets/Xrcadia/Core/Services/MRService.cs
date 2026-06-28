using System;
using System.Threading.Tasks;

namespace Xrcadia.Core.Services
{
    /// <summary>
    /// Result of an MR play-space check (XRC-94): the three conditions a tabletop run needs.
    /// </summary>
    public readonly struct MRSpaceStatus
    {
        public readonly bool Tracking;
        public readonly bool Passthrough;
        public readonly bool Surface;

        public MRSpaceStatus(bool tracking, bool passthrough, bool surface)
        {
            Tracking = tracking;
            Passthrough = passthrough;
            Surface = surface;
        }

        /// <summary>All three conditions met — safe to anchor the tabletop and begin.</summary>
        public bool IsReady => Tracking && Passthrough && Surface;

        public static MRSpaceStatus Ready => new MRSpaceStatus(true, true, true);
    }

    /// <summary>
    /// Reports whether the MR play space is usable (XRC-94). The actual OpenXR space/passthrough
    /// query is injected so this stays engine-agnostic and testable; with no checker provided
    /// (editor/headless) it reports <see cref="MRSpaceStatus.Ready"/>.
    /// </summary>
    public sealed class MRService : IAppService
    {
        private readonly Func<Task<MRSpaceStatus>> _check;

        public MRService(Func<Task<MRSpaceStatus>> check = null)
        {
            _check = check;
        }

        public string Name => "MR";

        public Task Initialize() => Task.CompletedTask;

        public Task<MRSpaceStatus> CheckSpace()
            => _check != null ? _check() : Task.FromResult(MRSpaceStatus.Ready);

        public void Shutdown() { }
    }
}

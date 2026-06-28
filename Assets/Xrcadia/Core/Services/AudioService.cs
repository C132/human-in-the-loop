using System.Threading.Tasks;

namespace Xrcadia.Core.Services
{
    public enum UiSound
    {
        Navigate,
        Confirm,
        Back,
    }

    /// <summary>
    /// Minimal audio service initialized at Boot. UI sound hooks are no-ops for now
    /// (audio mixer + banks land with a later audio issue); having the seam in place lets
    /// states/screens request sounds without churn later.
    /// </summary>
    public sealed class AudioService : IAppService
    {
        public string Name => "Audio";

        public Task Initialize() => Task.CompletedTask;

        public void PlayUi(UiSound sound)
        {
            // TODO: route to the audio mixer once banks exist.
        }

        public void Shutdown() { }
    }
}

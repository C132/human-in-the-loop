using System.Threading.Tasks;

namespace Xrcadia.Core.Services
{
    /// <summary>
    /// A core service initialized at Boot behind the Loading overlay (save, settings,
    /// audio, XR). No gameplay assets are touched here.
    /// </summary>
    public interface IAppService
    {
        string Name { get; }

        /// <summary>Async one-time init at Boot.</summary>
        Task Initialize();

        /// <summary>Tear down on Shutdown. Called in reverse registration order.</summary>
        void Shutdown();
    }
}

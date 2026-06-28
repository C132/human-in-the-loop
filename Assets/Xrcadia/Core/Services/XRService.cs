using System;
using System.Threading.Tasks;

namespace Xrcadia.Core.Services
{
    /// <summary>
    /// Brings the XR runtime up at Boot. The actual OpenXR loader start is injected so this
    /// stays engine-agnostic and testable; the App layer supplies the real start routine.
    /// If no starter is provided (editor/headless) it succeeds as a no-op.
    /// </summary>
    public sealed class XRService : IAppService
    {
        readonly Func<Task<bool>> _startXr;

        public XRService(Func<Task<bool>> startXr = null)
        {
            _startXr = startXr;
        }

        public string Name => "XR";

        public bool Active { get; private set; }

        public async Task Initialize()
        {
            if (_startXr == null)
            {
                Active = false;
                return;
            }

            Active = await _startXr();
        }

        public void Shutdown()
        {
            Active = false;
        }
    }
}

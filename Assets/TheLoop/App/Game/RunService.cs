using System.Threading.Tasks;
using Xrcadia.Core.Services;

namespace TheLoop.Game
{
    /// <summary>
    /// Holds the live <see cref="RunDriver"/> for the in-flight run (XRC-104) so the Session screen
    /// can render the board and route the player's tile placements into it. Set by the Session
    /// state on entry and cleared on exit.
    /// </summary>
    public sealed class RunService : IAppService
    {
        public RunDriver Current { get; set; }

        public string Name => "Run";

        public Task Initialize() => Task.CompletedTask;

        public void Shutdown() => Current = null;
    }
}

using Xrcadia.Core.Services;
using Xrcadia.Core.Transitions;

namespace Xrcadia.Core.StateMachine
{
    /// <summary>
    /// Everything a state (or a screen) needs to do its job: the services, the machine to
    /// drive navigation, and the shared loading-progress model. Passed into every
    /// <see cref="IGameState.Enter"/>.
    /// </summary>
    public sealed class StateContext
    {
        public IGameStateMachine Machine { get; }
        public ServiceRegistry Services { get; }
        public LoadingProgress Loading { get; }

        public StateContext(IGameStateMachine machine, ServiceRegistry services, LoadingProgress loading)
        {
            Machine = machine;
            Services = services;
            Loading = loading;
        }

        // Convenience accessors for the services the front-end touches constantly.
        public SaveService Save => Services.Get<SaveService>();
        public SettingsService Settings => Services.Get<SettingsService>();
        public AudioService Audio => Services.Get<AudioService>();
    }
}

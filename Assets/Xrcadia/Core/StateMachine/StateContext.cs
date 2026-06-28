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

        /// <summary>The active error (XRC-99), bound by the error screens. Null when healthy.</summary>
        public ErrorContext Error { get; } = new ErrorContext();

        /// <summary>The in-flight run and its last outcome (XRC-95); read by Session/Results.</summary>
        public RunReport Run { get; } = new RunReport();

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
        public MRService MR => Services.Get<MRService>();
    }
}

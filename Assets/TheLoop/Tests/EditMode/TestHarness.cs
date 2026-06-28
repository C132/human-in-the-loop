using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xrcadia.App.States;
using Xrcadia.Core.Services;
using Xrcadia.Core.StateMachine;
using Xrcadia.Core.Transitions;

namespace Xrcadia.Tests.EditMode
{
    /// <summary>
    /// Builds a fully-registered <see cref="GameStateManager"/> with the real app states and
    /// in-memory services, so the loading flow can be driven headlessly — no scene, no UI.
    /// </summary>
    internal sealed class TestHarness
    {
        public GameStateManager Machine { get; }
        public ServiceRegistry Services { get; }
        public SettingsService Settings { get; }
        public SaveService Save { get; }
        public SpyService Spy { get; }
        public List<StateChange> Changes { get; } = new List<StateChange>();
        public bool QuitCalled { get; private set; }

        public TestHarness(bool onboarded = false, bool hasSave = false)
        {
            var store = new InMemoryKeyValueStore();
            if (onboarded) store.SetInt("hitl.onboarding.completed", 1);
            if (hasSave) store.SetInt("hitl.save.exists", 1);

            Services = new ServiceRegistry();
            Settings = new SettingsService(store);
            Save = new SaveService(store);
            Spy = new SpyService();
            Services.Register(Settings);
            Services.Register(Save);
            Services.Register(Spy);

            Machine = new GameStateManager(Services, new LoadingProgress())
            {
                MinLoadingDwellSeconds = 0f,        // no dwell in tests
                DelaySeconds = _ => Task.CompletedTask,
            };

            Machine.Register(new BootState());
            Machine.Register(new SplashState());
            Machine.Register(new TitleState());
            Machine.Register(new OnboardingState());
            Machine.Register(new MainMenuState());
            Machine.Register(new HubState());
            Machine.Register(new ShutdownState(() => QuitCalled = true));
            Machine.Register(new LoadingOverlayState());
            Machine.Register(new SettingsOverlayState());

            Machine.Transitioned += c => Changes.Add(c);
        }

        public bool Visited(GameState state) => Changes.Exists(c => c.To == state);

        /// <summary>Async work matching what BootState runs (init services behind Loading).</summary>
        public Func<ITransitionReporter, Task> BootWork => r => Services.InitializeAll(r);
    }

    /// <summary>Service that records init/shutdown so tests can assert boot/teardown ran.</summary>
    internal sealed class SpyService : IAppService
    {
        public bool Initialized { get; private set; }
        public bool ShutDown { get; private set; }

        public string Name => "Spy";
        public Task Initialize() { Initialized = true; return Task.CompletedTask; }
        public void Shutdown() { ShutDown = true; }
    }
}

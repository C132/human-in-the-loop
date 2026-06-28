using UnityEngine;
using TheLoop.App.Screens;
using TheLoop.App.States;
using Xrcadia.Core.Services;
using Xrcadia.Core.StateMachine;
using Xrcadia.Core.Transitions;
using Xrcadia.UI;

namespace TheLoop.App
{
    /// <summary>
    /// Composition root. Runs automatically after the scene loads (no manual steps), wires the
    /// FSM + services + world-space UI router into a persistent object, and starts the cold-boot
    /// flow. This is the decision called out in XRC-87: a single persistent bootstrap object
    /// with state-driven world-space UI panels — no additive scene loading for Boot → MainMenu
    /// (no gameplay assets are involved yet).
    /// </summary>
    public static class Bootstrap
    {
        private static GameRunner Runner;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Launch()
        {
            if (Runner != null)
            {
                return; // Already booted (guards against domain-reload re-entry).
            }

            // --- Persistence + services ---
            var store = new PlayerPrefsKeyValueStore();
            var services = new ServiceRegistry();
            services.Register(new SettingsService(store));
            services.Register(new SaveService(store));
            services.Register(new AudioService());
            services.Register(new XRService(/* OpenXR starter wired in XRC-94 */));

            // --- State machine ---
            var loading = new LoadingProgress();
            var machine = new GameStateManager(services, loading);
            machine.DelaySeconds = async seconds => await Awaitable.WaitForSecondsAsync(seconds);

            RegisterStates(machine);

            // --- World-space UI ---
            var host = WorldSpaceUIHost.Create();
            var router = new UIRouter(host);
            RegisterScreens(router, machine.Context);
            router.Bind(machine);

            // --- Persistent runner ---
            var go = new GameObject("HitL.GameRunner");
            Object.DontDestroyOnLoad(go);
            host.transform.SetParent(go.transform, true);
            Runner = go.AddComponent<GameRunner>();
            Runner.Initialize(machine, services, router);

            // --- Go ---
            machine.Start().Forget();
        }

        private static void RegisterStates(GameStateManager machine)
        {
            machine.Register(new BootState());
            machine.Register(new SplashState());
            machine.Register(new TitleState());
            machine.Register(new OnboardingState());
            machine.Register(new MainMenuState());
            machine.Register(new HubState());
            machine.Register(new ShutdownState());
            machine.Register(new LoadingOverlayState());
            machine.Register(new SettingsOverlayState());
        }

        private static void RegisterScreens(UIRouter router, StateContext context)
        {
            router.Register(new SplashScreen(), context);
            router.Register(new TitleScreen(), context);
            router.Register(new OnboardingScreen(), context);
            router.Register(new MainMenuScreen(), context);
            router.Register(new HubScreen(), context);
            router.Register(new LoadingScreen(), context);
            router.Register(new SettingsScreen(), context);
        }
    }
}

using System.Threading.Tasks;
using Xrcadia.Core.StateMachine;

namespace TheLoop.Tests.EditMode
{
    /// <summary>Shared helpers for driving the front-end flow in tests.</summary>
    internal static class Flow
    {
        /// <summary>Pump pending continuations until no transition is in flight.</summary>
        public static async Task WaitUntilIdle(TestHarness h)
        {
            var guard = 0;
            while (h.Machine.Transitioning && guard++ < 10000)
            {
                await Task.Yield();
            }
        }

        /// <summary>Cold boot through to an interactive Main Menu, mirroring the real driver.</summary>
        public static async Task DriveToMainMenu(TestHarness h)
        {
            await h.Machine.Start();                                      // -> Boot
            await h.Machine.TransitionTo(GameState.Splash, h.BootWork);   // init services -> Splash
            await h.Machine.GoTo(GameState.Title);                        // splash done -> Title
            await h.Machine.Fire(GameTrigger.StartPressed);              // -> Onboarding (first run) or MainMenu

            if (h.Machine.CurrentBaseState == GameState.Onboarding)
            {
                await h.Machine.Fire(GameTrigger.OnboardingComplete);    // -> MainMenu
            }

            await WaitUntilIdle(h);
        }

        /// <summary>Cold boot through to the Hub via New Game (creates a profile, enters Hub).</summary>
        public static async Task DriveToHub(TestHarness h)
        {
            await DriveToMainMenu(h);
            await h.Machine.Fire(GameTrigger.NewGame);
            await WaitUntilIdle(h);
        }
    }
}

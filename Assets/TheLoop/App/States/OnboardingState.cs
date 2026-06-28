using System.Threading.Tasks;
using Xrcadia.Core.StateMachine;

namespace TheLoop.App.States
{
    /// <summary>
    /// First-run placeholder. Onboarding *content* is owned by XRC-90; this state only proves
    /// the once-only routing: it marks onboarding complete on entry so subsequent launches
    /// skip straight from Title to Main Menu, and exposes OnboardingComplete to continue.
    /// </summary>
    public sealed class OnboardingState : GameStateBase
    {
        public override GameState Id => GameState.Onboarding;

        public override Task Enter(StateContext context)
        {
            base.Enter(context);
            context.Settings.MarkOnboardingComplete();
            return Task.CompletedTask;
        }
    }
}

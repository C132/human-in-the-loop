using System.Threading.Tasks;
using Xrcadia.Core.StateMachine;

namespace TheLoop.App.States
{
    /// <summary>
    /// First-run state (XRC-90). Owns the once-only routing: it marks onboarding complete on
    /// entry so subsequent launches skip straight from Title to Main Menu, and resolves
    /// OnboardingComplete (fired by the screen on finish or skip) onward to Main Menu. The
    /// teaching sequence itself lives in <see cref="OnboardingContent"/> +
    /// <see cref="Screens.OnboardingScreen"/>; marking complete on entry is what makes a skip
    /// still count as onboarded.
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

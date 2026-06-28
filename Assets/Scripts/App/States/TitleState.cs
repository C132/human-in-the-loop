using Xrcadia.Core.StateMachine;

namespace Xrcadia.App.States
{
    /// <summary>
    /// "Look / press to start" gate. On the primary action, routes to Onboarding on first
    /// launch (once) or straight to the Main Menu otherwise. Both edges are declared legal in
    /// the transition table; the first-launch decision lives here.
    /// </summary>
    public sealed class TitleState : GameStateBase, ITriggerHandler
    {
        public override GameState Id => GameState.Title;

        public bool TryHandleTrigger(GameTrigger trigger)
        {
            if (trigger != GameTrigger.StartPressed)
            {
                return false;
            }

            var target = Context.Settings.IsFirstLaunch ? GameState.Onboarding : GameState.MainMenu;
            Context.Machine.GoTo(target).Forget();
            return true;
        }
    }
}

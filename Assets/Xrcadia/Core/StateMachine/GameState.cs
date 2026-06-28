namespace Xrcadia.Core.StateMachine
{
    /// <summary>
    /// The complete app state set (XRC-87). Base states replace one another on the
    /// state machine; overlay states push/pop on a stack above the current base state.
    /// Session sub-FSM states (Seeding/Shaping/...) are owned by XRC-95 and intentionally
    /// not modelled here.
    /// </summary>
    public enum GameState
    {
        None = 0,

        // Base (App) states.
        Boot,
        Splash,
        Title,
        Onboarding,
        MainMenu,
        Hub,
        MRSetup,
        Session,
        Results,
        Shutdown,

        // Overlay states (push/pop on a stack, render above the base state).
        Loading,
        Paused,
        Settings,
        ErrorModal,
    }

    public static class GameStateExtensions
    {
        public static bool IsOverlay(this GameState state)
        {
            switch (state)
            {
                case GameState.Loading:
                case GameState.Paused:
                case GameState.Settings:
                case GameState.ErrorModal:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsBase(this GameState state)
        {
            return state != GameState.None && !state.IsOverlay();
        }
    }
}

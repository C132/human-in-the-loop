using Xrcadia.Core.StateMachine;
using Debug = UnityEngine.Debug;

namespace TheLoop.App.States
{
    /// <summary>
    /// Root of the front-end (XRC-89 slice). Each option fires a state transition — no direct
    /// scene loads. Continue/New Game run their async save work behind the Loading overlay;
    /// Settings pushes an overlay; Quit goes to Shutdown via the transition table.
    /// </summary>
    public sealed class MainMenuState : GameStateBase, ITriggerHandler
    {
        public override GameState Id => GameState.MainMenu;

        public bool TryHandleTrigger(GameTrigger trigger)
        {
            switch (trigger)
            {
                case GameTrigger.Continue:
                    if (!Context.Save.HasSave)
                    {
                        Debug.LogWarning("[MainMenu] Continue fired with no save; ignored.");
                        return true;
                    }

                    Context.Machine.TransitionTo(GameState.Hub, reporter =>
                        Context.Save.Load(reporter)).Forget();
                    return true;

                case GameTrigger.NewGame:
                    // CreateNew overwrites any existing profile; the Main Menu screen gates this
                    // behind an overwrite confirmation when a save already exists (XRC-92).
                    Context.Machine.TransitionTo(GameState.Hub, reporter =>
                        Context.Save.CreateNew(reporter)).Forget();
                    return true;

                case GameTrigger.OpenSettings:
                    Context.Machine.PushOverlay(GameState.Settings).Forget();
                    return true;

                default:
                    return false; // Quit falls through to the transition table (→ Shutdown).
            }
        }
    }
}

using Xrcadia.Core.StateMachine;

namespace TheLoop.App.States
{
    /// <summary>
    /// Placeholder landing state so Continue / New Game have a defined destination (no
    /// dead-ends). The real Hub / Lab shell is XRC-93; this only confirms the transition
    /// landed and offers a route back to the Main Menu.
    /// </summary>
    public sealed class HubState : GameStateBase
    {
        public override GameState Id => GameState.Hub;
    }
}

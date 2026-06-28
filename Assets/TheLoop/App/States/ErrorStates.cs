using System.Threading.Tasks;
using Xrcadia.Core.StateMachine;
using Debug = UnityEngine.Debug;

namespace TheLoop.App.States
{
    /// <summary>
    /// Recoverable error overlay (XRC-99). Behaviorless host for the error screen, which offers
    /// Resume (pop → resume the paused base) or Safe Exit. Pushed over whatever state failed, so
    /// the underlying state is preserved and resumes intact when the issue is resolved.
    /// </summary>
    public sealed class ErrorModalState : GameStateBase
    {
        public override GameState Id => GameState.ErrorModal;
    }

    /// <summary>
    /// Fatal error landing state (XRC-99). On entry it logs the failure and protects the save by
    /// not writing anything (a half-written profile would compound the problem). The screen
    /// offers a single calm route out — Return to Main Menu — so a fatal error never dead-ends.
    /// </summary>
    public sealed class FatalErrorState : GameStateBase
    {
        public override GameState Id => GameState.Fatal;

        public override Task Enter(StateContext context)
        {
            base.Enter(context);

            var error = context.Error.Current;
            Debug.LogError($"[Fatal] {error?.Source}: {error?.Message}");
            // Save protection: deliberately do not touch the store here.
            return Task.CompletedTask;
        }
    }
}

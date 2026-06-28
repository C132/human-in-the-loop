using System.Threading.Tasks;
using Xrcadia.Core.StateMachine;

namespace TheLoop.App.States
{
    /// <summary>
    /// Wraps a single run (XRC-95). Owns the run sub-FSM and its context, publishes the live
    /// machine on <c>StateContext.Run</c> so the screen can render the phase, and — when the run
    /// reaches a terminal phase — records the outcome and exits to Results. Gameplay (what fires
    /// the sub-FSM signals) is out of scope (XRC-78). Pause is a push overlay (XRC-96), so the
    /// sub-state survives pause/resume; tracking loss routes through the recoverable error path.
    /// </summary>
    public sealed class SessionState : GameStateBase
    {
        public RunSubMachine Run { get; private set; }

        public override GameState Id => GameState.Session;

        public override Task Enter(StateContext context)
        {
            base.Enter(context);

            var seed = context.Save.Profile?.runsCompleted ?? 0;
            Run = new RunSubMachine(new RunContext { Seed = seed });
            context.Run.Active = Run;
            Run.PhaseChanged += OnPhaseChanged;
            return Task.CompletedTask;
        }

        public override Task Exit()
        {
            if (Run != null)
            {
                Run.PhaseChanged -= OnPhaseChanged;
            }

            return base.Exit();
        }

        private void OnPhaseChanged(RunPhase phase)
        {
            if (!Run.IsComplete) return;

            // Hand the outcome to Results and leave the run.
            Context.Run.LastOutcome = Run.BuildOutcome();
            Context.Run.Active = null;
            Context.Machine.GoTo(GameState.Results).Forget();
        }
    }
}

using System.Threading.Tasks;
using TheLoop.Game;
using Xrcadia.Core.StateMachine;

namespace TheLoop.App.States
{
    /// <summary>
    /// Wraps a single run (XRC-95 / XRC-103). Builds the board (seeded), loads the agent's trained
    /// memory from the save, and drives a <see cref="RunDriver"/> on a comfortable cadence — the
    /// driver interleaves shaping with the agent's autonomous steps and mirrors the phase onto the
    /// run sub-FSM. When the run resolves it persists the updated memory to the profile, records
    /// the outcome and exits to Results. Gameplay rules live in TheLoop.Game.
    /// </summary>
    public sealed class SessionState : GameStateBase
    {
        private const float StepSeconds = 0.35f; // MR-comfortable step cadence

        private float _accum;
        private bool _started;

        public RunSubMachine Run { get; private set; }
        public RunDriver Driver { get; private set; }

        public override GameState Id => GameState.Session;

        public override Task Enter(StateContext context)
        {
            base.Enter(context);

            var profile = context.Save.Profile;
            var seed = profile?.runsCompleted ?? 0;
            var memory = AgentMemory.Deserialize(profile?.agentMemory);
            var board = RunBoard.Seeded(seed);

            Run = new RunSubMachine(new RunContext { Seed = seed });
            Driver = new RunDriver(Run, board, memory);
            context.Run.Active = Run;
            if (context.Services.TryGet<RunService>(out var run)) run.Current = Driver;
            Run.PhaseChanged += OnPhaseChanged;

            _accum = 0f;
            _started = false;
            return Task.CompletedTask;
        }

        public override void Tick(float deltaTime)
        {
            if (Run == null || Run.IsComplete) return;

            if (!_started)
            {
                Driver.Start();
                _started = true;
                return;
            }

            _accum += deltaTime;
            if (_accum < StepSeconds) return;

            _accum = 0f;
            Driver.Tick();
        }

        public override Task Exit()
        {
            if (Run != null)
            {
                Run.PhaseChanged -= OnPhaseChanged;
            }

            if (Context.Services.TryGet<RunService>(out var run)) run.Current = null;
            return base.Exit();
        }

        private void OnPhaseChanged(RunPhase phase)
        {
            if (!Run.IsComplete) return;

            // Persist the agent's updated memory — the refine loop — before leaving the run.
            var profile = Context.Save.Profile;
            if (profile != null && Driver != null)
            {
                profile.agentMemory = Driver.Memory.Serialize();
                Context.Save.Save();
            }

            Context.Run.LastOutcome = Run.BuildOutcome();
            Context.Run.Active = null;
            Context.Machine.GoTo(GameState.Results).Forget();
        }
    }
}

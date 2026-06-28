using System;

namespace Xrcadia.Core.StateMachine
{
    /// <summary>The phases of a single run (XRC-95), nested inside the Session base state.</summary>
    public enum RunPhase
    {
        Seeding,        // world generation
        Shaping,        // player places terrain / hazards / rewards
        AgentRunning,   // agent acts on its learned policy
        Resolving,      // run is ending — tallying the result
        RunSuccess,     // terminal: goal reached
        RunFailed,      // terminal: agent down
    }

    public enum RunResult
    {
        Success,
        Failure,
    }

    /// <summary>The single source of truth for a run while it is in flight.</summary>
    public sealed class RunContext
    {
        public int Seed;
        public int PlacedTiles;
        public int Score;
        public bool AgentAlive = true;
    }

    /// <summary>The payload the Session hands to Results (success/failure + tallies).</summary>
    public sealed class RunOutcome
    {
        public RunResult Result;
        public int Score;
        public int PlacedTiles;
    }

    /// <summary>
    /// The committed debrief Results produces from an outcome (XRC-97): the payout awarded and a
    /// readable "what the agent learned" recap. Bound by the Results screen for display.
    /// </summary>
    public sealed class RunDebrief
    {
        public RunResult Result;
        public int CurrencyAwarded;
        public int XpAwarded;
        public string LearningRecap;
    }

    /// <summary>
    /// Shared run holder on <see cref="StateContext"/>: the live sub-machine while a run is in
    /// flight, the outcome the Results state consumes, and the debrief it produces.
    /// </summary>
    public sealed class RunReport
    {
        public RunSubMachine Active { get; set; }
        public RunOutcome LastOutcome { get; set; }
        public RunDebrief LastDebrief { get; set; }
    }

    /// <summary>
    /// The run sub-FSM (XRC-95). Engine-free and signal-driven so it is unit-testable; the
    /// gameplay that fires the signals is out of scope (XRC-78). Shaping and AgentRunning
    /// interleave — the core tension — rather than running in strict sequence. Each signal is a
    /// guarded transition that no-ops (returns false) from the wrong phase.
    /// </summary>
    public sealed class RunSubMachine
    {
        public RunSubMachine(RunContext context)
        {
            Context = context ?? new RunContext();
        }

        public RunContext Context { get; }
        public RunPhase Phase { get; private set; } = RunPhase.Seeding;

        public event Action<RunPhase> PhaseChanged;

        public bool IsComplete => Phase == RunPhase.RunSuccess || Phase == RunPhase.RunFailed;

        /// <summary>World generation finished — open shaping.</summary>
        public bool WorldReady() => Move(RunPhase.Seeding, RunPhase.Shaping);

        /// <summary>Let the agent act on the loop as currently shaped.</summary>
        public bool AdvanceAgent() => Move(RunPhase.Shaping, RunPhase.AgentRunning);

        /// <summary>Pause the agent to keep placing — back to shaping (the interleave).</summary>
        public bool KeepPlacing() => Move(RunPhase.AgentRunning, RunPhase.Shaping);

        /// <summary>Goal reached — resolve toward success.</summary>
        public bool ReachGoal()
        {
            if (!Move(RunPhase.AgentRunning, RunPhase.Resolving)) return false;
            Context.AgentAlive = true;
            return true;
        }

        /// <summary>Agent down — resolve toward failure.</summary>
        public bool AgentDown()
        {
            if (!Move(RunPhase.AgentRunning, RunPhase.Resolving)) return false;
            Context.AgentAlive = false;
            return true;
        }

        /// <summary>Finalize a resolving run into success or failure.</summary>
        public bool Settle()
        {
            if (Phase != RunPhase.Resolving) return false;
            Set(Context.AgentAlive ? RunPhase.RunSuccess : RunPhase.RunFailed);
            return true;
        }

        public RunOutcome BuildOutcome() => new RunOutcome
        {
            Result = Phase == RunPhase.RunSuccess ? RunResult.Success : RunResult.Failure,
            Score = Context.Score,
            PlacedTiles = Context.PlacedTiles,
        };

        private bool Move(RunPhase from, RunPhase to)
        {
            if (Phase != from) return false;
            Set(to);
            return true;
        }

        private void Set(RunPhase to)
        {
            Phase = to;
            PhaseChanged?.Invoke(to);
        }
    }
}

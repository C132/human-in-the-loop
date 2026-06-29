using System.Collections.Generic;
using Xrcadia.Core.StateMachine;

namespace TheLoop.Game
{
    /// <summary>Tunables for a run (XRC-103).</summary>
    public sealed class RunConfig
    {
        public int MaxSteps = 64;
        public int PlacementCap = 3;       // how many placements can bank up
        public int PlacementsPerStep = 1;  // budget granted each agent step
        public int GoalBonus = 10;
    }

    /// <summary>
    /// Drives one run end to end (XRC-103): owns the board, agent and memory, interleaves player
    /// shaping with the agent's autonomous steps, resolves into Success/Failure, and mirrors the
    /// run's phase onto the <see cref="RunSubMachine"/> so the FSM/UI track it. Plain C# and
    /// tick-driven, so it is unit-testable and the Session screen just advances it on a timer.
    /// </summary>
    public sealed class RunDriver
    {
        private readonly RunSubMachine _sub;
        private readonly RunConfig _cfg;

        public RunDriver(RunSubMachine sub, RunBoard board, AgentMemory memory, RunConfig config = null)
        {
            _sub = sub;
            _cfg = config ?? new RunConfig();
            Board = board;
            Memory = memory;
            Agent = new Agent(board.Start);
            Placements = _cfg.PlacementCap;
            Plan = AgentPolicy.Plan(board, Agent, memory);
        }

        public RunBoard Board { get; }
        public Agent Agent { get; }
        public AgentMemory Memory { get; }

        public int Score { get; private set; }
        public int Steps { get; private set; }
        public int PlacedTiles { get; private set; }
        public int Placements { get; private set; }

        /// <summary>The agent's current intended step + reason (the transparent-agent readout).</summary>
        public StepPlan Plan { get; private set; }

        public bool IsComplete => _sub.IsComplete;

        /// <summary>Open shaping (Seeding → Shaping). Call once when the run begins.</summary>
        public void Start() => _sub.WorldReady();

        /// <summary>
        /// Player shaping during the run: drop a Hazard/Reward/Wall on an empty cell, paced by the
        /// placement budget. A wall that would strand the agent from the goal is rejected (no
        /// soft-locks). Returns whether the placement landed.
        /// </summary>
        public bool TryPlace(Coord c, CellType type)
        {
            if (IsComplete || Placements <= 0) return false;
            if (type != CellType.Hazard && type != CellType.Reward && type != CellType.Wall) return false;
            if (!Board.InBounds(c) || Board.Get(c) != CellType.Empty) return false;

            Board.Set(c, type);
            if (type == CellType.Wall && !PathExists(Board, Agent.Position, Board.Goal))
            {
                Board.Set(c, CellType.Empty); // reverting keeps the run solvable
                return false;
            }

            Placements--;
            PlacedTiles++;
            Plan = AgentPolicy.Plan(Board, Agent, Memory);
            return true;
        }

        /// <summary>Advance the run by one agent step (with the shaping window around it).</summary>
        public void Tick()
        {
            if (IsComplete) return;

            _sub.AdvanceAgent(); // Shaping → AgentRunning

            var plan = AgentPolicy.Plan(Board, Agent, Memory);
            var res = AgentPolicy.Resolve(Board, Agent, Memory, plan.Next);
            Score += res.RewardValue;
            Steps++;

            if (res.ReachedGoal)
            {
                Score += _cfg.GoalBonus;
                Resolve(true);
                return;
            }

            if (Agent.IsDown || Steps >= _cfg.MaxSteps)
            {
                Resolve(false);
                return;
            }

            _sub.KeepPlacing(); // AgentRunning → Shaping (reopen shaping)
            Placements = System.Math.Min(_cfg.PlacementCap, Placements + _cfg.PlacementsPerStep);
            Plan = AgentPolicy.Plan(Board, Agent, Memory);
        }

        private void Resolve(bool success)
        {
            _sub.Context.Score = Score;
            _sub.Context.PlacedTiles = PlacedTiles;
            _sub.Context.AgentAlive = success;

            if (success) _sub.ReachGoal();
            else _sub.AgentDown();
            _sub.Settle(); // → RunSuccess / RunFailed (terminal)
        }

        /// <summary>BFS reachability over walkable cells (walls block; hazards do not).</summary>
        private static bool PathExists(RunBoard board, Coord from, Coord to)
        {
            if (from == to) return true;
            var seen = new HashSet<Coord> { from };
            var queue = new Queue<Coord>();
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                foreach (var nb in board.Neighbours(cur))
                {
                    if (!board.IsWalkable(nb) || !seen.Add(nb)) continue;
                    if (nb == to) return true;
                    queue.Enqueue(nb);
                }
            }

            return false;
        }
    }
}

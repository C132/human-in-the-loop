using System.Collections.Generic;

namespace TheLoop.Game
{
    /// <summary>The agent's intended next move and a one-line, player-readable reason for it.</summary>
    public readonly struct StepPlan
    {
        public readonly Coord Next;
        public readonly string Reason;
        public readonly bool Waiting;

        public StepPlan(Coord next, string reason, bool waiting)
        {
            Next = next;
            Reason = reason;
            Waiting = waiting;
        }
    }

    /// <summary>What happened when a planned step was applied.</summary>
    public struct StepResolution
    {
        public bool Moved;
        public bool TookDamage;
        public bool CollectedReward;
        public bool ReachedGoal;
        public int RewardValue;
    }

    /// <summary>
    /// The autonomous, legible agent (XRC-102). Each step it walks the least-cost weighted path
    /// toward the goal, where a cell's enter-cost rises with hazards and learned danger and falls
    /// near rewards and learned reward. The path is recomputed every step so the agent reacts to
    /// tiles placed mid-run. It never crashes when boxed in. The player can always read its intent.
    /// </summary>
    public static class AgentPolicy
    {
        public const float HazardAvoid = 6f;       // how hard the agent steers around a hazard tile
        public const float RewardLure = 3f;        // how strongly a reward tile pulls it
        public const int HazardDamage = 1;
        public const float DangerMemoryGain = 4f;  // aversion learned from getting hurt on a cell
        public const float RewardMemoryGain = 2f;  // attraction learned from a reward on a cell
        public const int RewardValue = 5;
        public const float MinEnterCost = 0.2f;    // keep edge costs positive for Dijkstra

        /// <summary>Cost to step onto a cell, before the path search.</summary>
        public static float EnterCost(RunBoard board, AgentMemory memory, Coord c)
        {
            var cost = 1f;
            var t = board.Get(c);
            if (t == CellType.Hazard) cost += HazardAvoid;
            cost += memory.Danger(c);
            if (t == CellType.Reward) cost -= RewardLure;
            cost -= memory.Reward(c);
            return cost < MinEnterCost ? MinEnterCost : cost;
        }

        /// <summary>Decide the next cell to move to (and why), without mutating anything.</summary>
        public static StepPlan Plan(RunBoard board, Agent agent, AgentMemory memory)
        {
            var start = agent.Position;
            var goal = board.Goal;

            var next = FirstStepToGoal(board, start, goal, memory, out var reachable);

            if (!reachable)
            {
                // No route to the goal right now — edge toward it, or hold if fully boxed in.
                var fallback = BestNeighbourTowardGoal(board, start, goal);
                if (fallback == start) return new StepPlan(start, "no path — holding position", true);
                return new StepPlan(fallback, "searching for a route", false);
            }

            if (next == start) return new StepPlan(start, "at the goal", true);

            return new StepPlan(next, ReasonFor(board, memory, start, next, goal), false);
        }

        /// <summary>Apply a planned step: move, take damage / collect reward / reach goal, and learn.</summary>
        public static StepResolution Resolve(RunBoard board, Agent agent, AgentMemory memory, Coord next)
        {
            var res = new StepResolution();
            if (next == agent.Position) return res; // waited

            agent.Position = next;
            res.Moved = true;

            switch (board.Get(next))
            {
                case CellType.Hazard:
                    agent.Hp -= HazardDamage;
                    res.TookDamage = true;
                    memory.RememberDanger(next, DangerMemoryGain);
                    break;
                case CellType.Reward:
                    res.CollectedReward = true;
                    res.RewardValue = RewardValue;
                    memory.RememberReward(next, RewardMemoryGain);
                    board.Set(next, CellType.Empty); // consumed
                    break;
                case CellType.Goal:
                    res.ReachedGoal = true;
                    break;
            }

            return res;
        }

        // ---- internals ----

        private static Coord FirstStepToGoal(RunBoard board, Coord start, Coord goal, AgentMemory memory, out bool reachable)
        {
            // Dijkstra over walkable cells (small board, so an O(n^2) frontier scan is fine).
            var dist = new Dictionary<Coord, float> { [start] = 0f };
            var prev = new Dictionary<Coord, Coord>();
            var done = new HashSet<Coord>();

            while (true)
            {
                var hasU = false;
                var u = start;
                var best = float.MaxValue;
                foreach (var kv in dist)
                {
                    if (done.Contains(kv.Key) || kv.Value >= best) continue;
                    best = kv.Value;
                    u = kv.Key;
                    hasU = true;
                }

                if (!hasU) break;
                done.Add(u);
                if (u == goal) break;

                foreach (var nb in board.Neighbours(u))
                {
                    if (!board.IsWalkable(nb)) continue;
                    var nd = dist[u] + EnterCost(board, memory, nb);
                    if (!dist.TryGetValue(nb, out var old) || nd < old)
                    {
                        dist[nb] = nd;
                        prev[nb] = u;
                    }
                }
            }

            if (start == goal) { reachable = true; return start; }
            if (!prev.ContainsKey(goal)) { reachable = false; return start; }

            reachable = true;
            var node = goal;
            while (prev.TryGetValue(node, out var p))
            {
                if (p == start) return node;
                node = p;
            }

            return start;
        }

        private static Coord BestNeighbourTowardGoal(RunBoard board, Coord start, Coord goal)
        {
            var best = start;
            var bestDist = start.ManhattanTo(goal);
            foreach (var nb in board.Neighbours(start))
            {
                if (!board.IsWalkable(nb)) continue;
                var d = nb.ManhattanTo(goal);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = nb;
                }
            }

            return best;
        }

        private static string ReasonFor(RunBoard board, AgentMemory memory, Coord start, Coord next, Coord goal)
        {
            if (board.Get(next) == CellType.Reward || memory.Reward(next) > 0.01f)
            {
                return $"seeking reward at {next}";
            }

            // Did it pass up a closer cell because that cell is dangerous?
            foreach (var nb in board.Neighbours(start))
            {
                if (!board.IsWalkable(nb)) continue;
                if (nb.ManhattanTo(goal) >= next.ManhattanTo(goal)) continue;

                if (memory.Danger(nb) > 0.01f) return $"avoiding {nb} — hurt here before";
                if (board.Get(nb) == CellType.Hazard) return $"avoiding the hazard at {nb}";
            }

            return "advancing toward the goal";
        }
    }
}

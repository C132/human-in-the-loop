using NUnit.Framework;
using TheLoop.Game;

namespace TheLoop.Tests.EditMode
{
    /// <summary>XRC-102: agent memory (training) + the legible weighted-pathfinding policy.</summary>
    public sealed class AgentTests
    {
        private static RunBoard Lane()
        {
            // A 5x5 board with a straight lane: Start (1,2) -> Goal (4,2).
            var b = new RunBoard(5, 5);
            b.Designate(new Coord(1, 2), new Coord(4, 2));
            return b;
        }

        // ---- Memory ----

        [Test]
        public void Memory_RoundTrips_AndDecays_AndToleratesGarbage()
        {
            var m = new AgentMemory();
            m.RememberDanger(new Coord(2, 2), 4f);
            m.RememberReward(new Coord(3, 1), 2f);

            var restored = AgentMemory.Deserialize(m.Serialize());
            Assert.AreEqual(4f, restored.Danger(new Coord(2, 2)), 0.01f);
            Assert.AreEqual(2f, restored.Reward(new Coord(3, 1)), 0.01f);

            m.Decay(0.5f);
            Assert.AreEqual(2f, m.Danger(new Coord(2, 2)), 0.01f, "decay halves the weight");

            // Corruption tolerance: null/garbage yields empty memory, not a crash.
            Assert.AreEqual(0f, AgentMemory.Deserialize(null).Danger(new Coord(0, 0)));
            Assert.AreEqual(0f, AgentMemory.Deserialize("junk;;x,y,z,w").Danger(new Coord(1, 2)));
        }

        // ---- Cost ----

        [Test]
        public void EnterCost_RisesOnDanger_FallsOnReward_StaysPositive()
        {
            var b = Lane();
            var m = new AgentMemory();
            var empty = new Coord(2, 1);

            Assert.AreEqual(1f, AgentPolicy.EnterCost(b, m, empty), 0.01f);

            b.Set(new Coord(2, 0), CellType.Hazard);
            Assert.Greater(AgentPolicy.EnterCost(b, m, new Coord(2, 0)), 1f);

            b.Set(new Coord(2, 4), CellType.Reward);
            Assert.Less(AgentPolicy.EnterCost(b, m, new Coord(2, 4)), 1f);

            m.RememberReward(empty, 100f); // would go very negative
            Assert.GreaterOrEqual(AgentPolicy.EnterCost(b, m, empty), AgentPolicy.MinEnterCost);
        }

        // ---- Planning ----

        [Test]
        public void Plan_StepsTowardGoal_OnOpenBoard()
        {
            var b = Lane();
            var agent = new Agent(b.Start);

            var plan = AgentPolicy.Plan(b, agent, new AgentMemory());

            Assert.AreEqual(new Coord(2, 2), plan.Next, "straight down the open lane");
            Assert.Less(plan.Next.ManhattanTo(b.Goal), agent.Position.ManhattanTo(b.Goal));
            Assert.IsFalse(plan.Waiting);
        }

        [Test]
        public void Plan_RoutesAroundAHazard_WithReason()
        {
            var b = Lane();
            b.Set(new Coord(2, 2), CellType.Hazard); // straight in the agent's way
            var agent = new Agent(b.Start);

            var plan = AgentPolicy.Plan(b, agent, new AgentMemory());

            Assert.AreNotEqual(new Coord(2, 2), plan.Next, "does not step onto the hazard");
            StringAssert.StartsWith("avoiding", plan.Reason);
        }

        // ---- Resolution ----

        [Test]
        public void Resolve_Hazard_Reward_Goal()
        {
            // Hazard: lose 1 HP, remember the danger.
            var b = Lane();
            b.Set(new Coord(2, 2), CellType.Hazard);
            var agent = new Agent(b.Start);
            var mem = new AgentMemory();
            var haz = AgentPolicy.Resolve(b, agent, mem, new Coord(2, 2));
            Assert.IsTrue(haz.TookDamage);
            Assert.AreEqual(Agent.DefaultHp - 1, agent.Hp);
            Assert.Greater(mem.Danger(new Coord(2, 2)), 0f);

            // Reward: gain value, consume the cell, remember the reward.
            var b2 = Lane();
            b2.Set(new Coord(2, 2), CellType.Reward);
            var a2 = new Agent(b2.Start);
            var m2 = new AgentMemory();
            var rew = AgentPolicy.Resolve(b2, a2, m2, new Coord(2, 2));
            Assert.IsTrue(rew.CollectedReward);
            Assert.AreEqual(AgentPolicy.RewardValue, rew.RewardValue);
            Assert.AreEqual(CellType.Empty, b2.Get(new Coord(2, 2)));
            Assert.Greater(m2.Reward(new Coord(2, 2)), 0f);

            // Goal: reached.
            var b3 = Lane();
            var a3 = new Agent(new Coord(3, 2));
            var goal = AgentPolicy.Resolve(b3, a3, new AgentMemory(), b3.Goal);
            Assert.IsTrue(goal.ReachedGoal);
        }

        // ---- The differentiator: smarter across runs ----

        [Test]
        public void TrainedAgent_AvoidsWhereItWasHurt()
        {
            // Fresh agent walks straight onto the lane cell.
            var fresh = AgentPolicy.Plan(Lane(), new Agent(new Coord(1, 2)), new AgentMemory());
            Assert.AreEqual(new Coord(2, 2), fresh.Next);

            // The same agent, having been hurt on (2,2) in a past run, now routes around it.
            var trained = new AgentMemory();
            trained.RememberDanger(new Coord(2, 2), 10f);
            var plan = AgentPolicy.Plan(Lane(), new Agent(new Coord(1, 2)), trained);

            Assert.AreNotEqual(new Coord(2, 2), plan.Next, "the trained agent avoids the cell it died on");
            StringAssert.StartsWith("avoiding", plan.Reason);
        }
    }
}

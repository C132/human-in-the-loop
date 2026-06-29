using System.Threading.Tasks;
using NUnit.Framework;
using TheLoop.App.States;
using TheLoop.Game;
using Xrcadia.Core.StateMachine;

namespace TheLoop.Tests.EditMode
{
    /// <summary>XRC-103: the run driver (tick loop, resolution, shaping guard) + Session integration.</summary>
    public sealed class RunDriverTests
    {
        private sealed class PlaceholderState : GameStateBase
        {
            private readonly GameState _id;
            public PlaceholderState(GameState id) => _id = id;
            public override GameState Id => _id;
        }

        private static RunBoard Lane(int w = 5, int h = 5)
        {
            var b = new RunBoard(w, h);
            b.Designate(new Coord(0, h / 2), new Coord(w - 1, h / 2));
            return b;
        }

        private static RunDriver Drive(RunBoard board, AgentMemory mem = null, RunConfig cfg = null)
        {
            var d = new RunDriver(new RunSubMachine(new RunContext()), board, mem ?? new AgentMemory(), cfg);
            d.Start();
            var guard = 0;
            while (!d.IsComplete && guard++ < 500) d.Tick();
            return d;
        }

        [Test]
        public void SuccessfulRun_ReachesGoal_FillsContext()
        {
            var board = Lane();
            var sub = new RunSubMachine(new RunContext());
            var d = new RunDriver(sub, board, new AgentMemory());
            d.Start();
            var guard = 0;
            while (!d.IsComplete && guard++ < 500) d.Tick();

            Assert.AreEqual(RunPhase.RunSuccess, sub.Phase);
            Assert.AreEqual(board.Goal, d.Agent.Position);
            Assert.IsTrue(sub.Context.AgentAlive);
            Assert.AreEqual(d.Score, sub.Context.Score);
            Assert.GreaterOrEqual(d.Score, RunDefaultGoalBonus(), "goal bonus paid");
        }

        private static int RunDefaultGoalBonus() => new RunConfig().GoalBonus;

        [Test]
        public void Timeout_IsFailure()
        {
            var sub = new RunSubMachine(new RunContext());
            var d = new RunDriver(sub, Lane(), new AgentMemory(), new RunConfig { MaxSteps = 2 });
            d.Start();
            var guard = 0;
            while (!d.IsComplete && guard++ < 500) d.Tick();

            Assert.AreEqual(RunPhase.RunFailed, sub.Phase);
            Assert.IsFalse(sub.Context.AgentAlive);
        }

        [Test]
        public void HazardCorridor_KillsAgent_AndRecordsDanger()
        {
            var board = new RunBoard(5, 1);
            board.Designate(new Coord(0, 0), new Coord(4, 0));
            board.Set(new Coord(1, 0), CellType.Hazard);
            board.Set(new Coord(2, 0), CellType.Hazard);
            board.Set(new Coord(3, 0), CellType.Hazard);

            var d = Drive(board);

            Assert.IsTrue(d.Agent.IsDown);
            Assert.Greater(d.Memory.Danger(new Coord(1, 0)), 0f, "remembered the danger it crossed");
        }

        [Test]
        public void TryPlace_RejectsSealingTheGoal_AllowsHazard()
        {
            var board = new RunBoard(3, 1);
            board.Designate(new Coord(0, 0), new Coord(2, 0));
            var d = new RunDriver(new RunSubMachine(new RunContext()), board, new AgentMemory());
            d.Start();

            Assert.IsFalse(d.TryPlace(new Coord(1, 0), CellType.Wall), "a wall sealing the goal is rejected");
            Assert.AreEqual(CellType.Empty, board.Get(new Coord(1, 0)));
            Assert.IsTrue(d.TryPlace(new Coord(1, 0), CellType.Hazard), "a hazard is fine — it doesn't block");
            Assert.AreEqual(1, d.PlacedTiles);
        }

        [Test]
        public void TryPlace_RespectsBudget()
        {
            var d = new RunDriver(new RunSubMachine(new RunContext()), Lane(8, 8), new AgentMemory(),
                new RunConfig { PlacementCap = 1 });
            d.Start();

            Assert.IsTrue(d.TryPlace(new Coord(2, 5), CellType.Reward));
            Assert.IsFalse(d.TryPlace(new Coord(3, 5), CellType.Reward), "out of budget");
        }

        [Test]
        public void TrainedMemory_PersistsAcrossRuns_AndChangesBehaviour()
        {
            // Run 1: cross a single hazard (survives) and record its danger.
            var b1 = new RunBoard(5, 1);
            b1.Designate(new Coord(0, 0), new Coord(4, 0));
            b1.Set(new Coord(1, 0), CellType.Hazard);
            var d1 = Drive(b1);
            Assert.Greater(d1.Memory.Danger(new Coord(1, 0)), 0f);

            // Persist + reload (save round-trip).
            var memory = AgentMemory.Deserialize(d1.Memory.Serialize());

            // Run 2: open board; (1,0) is now safe terrain but remembered-bad, with a detour.
            var b2 = new RunBoard(5, 3);
            b2.Designate(new Coord(0, 0), new Coord(4, 0));

            var fresh = AgentPolicy.Plan(b2, new Agent(new Coord(0, 0)), new AgentMemory());
            Assert.AreEqual(new Coord(1, 0), fresh.Next, "an untrained agent walks straight");

            var trained = AgentPolicy.Plan(b2, new Agent(new Coord(0, 0)), memory);
            Assert.AreNotEqual(new Coord(1, 0), trained.Next, "the trained agent avoids where it was hurt");
        }

        [Test]
        public async Task Session_PlaysRun_PersistsMemory_ExitsToResults()
        {
            var h = new TestHarness(onboarded: true);
            var session = new SessionState();
            h.Machine.Register(new MRSetupState());
            h.Machine.Register(session);
            h.Machine.Register(new PlaceholderState(GameState.Results));

            await Flow.DriveToHub(h);
            await h.Machine.Fire(GameTrigger.LaunchRun);
            await h.Machine.Fire(GameTrigger.BeginSession);
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Session, h.Machine.CurrentBaseState);

            // Drive the live run to completion through the Session's own driver.
            session.Driver.Start();
            var guard = 0;
            while (!session.Driver.IsComplete && guard++ < 500) session.Driver.Tick();
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Results, h.Machine.CurrentBaseState, "a finished run exits to Results");
            Assert.IsNotNull(h.Machine.Context.Run.LastOutcome);
            Assert.IsNotNull(h.Save.Profile.agentMemory, "the agent's memory was persisted to the profile");
        }
    }
}

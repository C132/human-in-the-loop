using NUnit.Framework;
using Xrcadia.Core.StateMachine;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-95 run sub-FSM: the full Seeding→…→Resolving→(Success|Failed) drive, the
    /// Shaping ⇄ AgentRunning interleave, and rejection of out-of-phase signals.
    /// </summary>
    public sealed class RunSubMachineTests
    {
        [Test]
        public void FullRun_ReachGoal_EndsInSuccess()
        {
            var run = new RunSubMachine(new RunContext());
            Assert.AreEqual(RunPhase.Seeding, run.Phase);

            Assert.IsTrue(run.WorldReady());
            Assert.AreEqual(RunPhase.Shaping, run.Phase);
            Assert.IsTrue(run.AdvanceAgent());
            Assert.AreEqual(RunPhase.AgentRunning, run.Phase);
            Assert.IsTrue(run.ReachGoal());
            Assert.AreEqual(RunPhase.Resolving, run.Phase);
            Assert.IsTrue(run.Settle());

            Assert.AreEqual(RunPhase.RunSuccess, run.Phase);
            Assert.IsTrue(run.IsComplete);
            Assert.AreEqual(RunResult.Success, run.BuildOutcome().Result);
        }

        [Test]
        public void FullRun_AgentDown_EndsInFailure()
        {
            var run = new RunSubMachine(new RunContext());
            run.WorldReady();
            run.AdvanceAgent();

            Assert.IsTrue(run.AgentDown());
            Assert.AreEqual(RunPhase.Resolving, run.Phase);
            Assert.IsTrue(run.Settle());

            Assert.AreEqual(RunPhase.RunFailed, run.Phase);
            Assert.AreEqual(RunResult.Failure, run.BuildOutcome().Result);
        }

        [Test]
        public void ShapingAndAgentRunning_Interleave()
        {
            var run = new RunSubMachine(new RunContext());
            run.WorldReady();

            // Place, run, place again, run again — not a strict one-way sequence.
            Assert.IsTrue(run.AdvanceAgent());   // Shaping -> AgentRunning
            Assert.IsTrue(run.KeepPlacing());    // AgentRunning -> Shaping
            Assert.IsTrue(run.AdvanceAgent());   // Shaping -> AgentRunning
            Assert.IsTrue(run.KeepPlacing());    // AgentRunning -> Shaping
            Assert.AreEqual(RunPhase.Shaping, run.Phase);
        }

        [Test]
        public void OutOfPhaseSignals_AreRejected()
        {
            var run = new RunSubMachine(new RunContext());

            Assert.IsFalse(run.AdvanceAgent(), "cannot advance the agent while seeding");
            Assert.IsFalse(run.ReachGoal(), "cannot reach the goal before the agent runs");
            Assert.IsFalse(run.Settle(), "cannot settle before resolving");
            Assert.AreEqual(RunPhase.Seeding, run.Phase, "rejected signals leave the phase unchanged");
        }
    }
}

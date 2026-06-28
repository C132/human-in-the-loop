using System.Threading.Tasks;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Xrcadia.Core.StateMachine;

namespace Xrcadia.Tests.EditMode
{
    /// <summary>FSM mechanics (XRC-87): guards, overlay stack, re-entrancy.</summary>
    public sealed class FsmCoreTests
    {
        [Test]
        public async Task IllegalTransition_IsRejectedAndLogged()
        {
            var h = new TestHarness(onboarded: true);
            await Flow.DriveToMainMenu(h);

            LogAssert.Expect(LogType.Error, new Regex("Illegal transition"));
            await h.Machine.GoTo(GameState.Session); // MainMenu -> Session is not allowed

            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentBaseState);
        }

        [Test]
        public async Task UnregisteredState_IsRejectedAndLogged()
        {
            var h = new TestHarness(onboarded: true);
            await Flow.DriveToMainMenu(h);

            // MRSetup edge from Hub exists in the table but no state is registered.
            await h.Machine.Fire(GameTrigger.NewGame);
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Hub, h.Machine.CurrentBaseState);

            LogAssert.Expect(LogType.Error, new Regex("No state registered for MRSetup"));
            await h.Machine.GoTo(GameState.MRSetup);
            Assert.AreEqual(GameState.Hub, h.Machine.CurrentBaseState);
        }

        [Test]
        public async Task Overlay_PushAndPop_RestoresBase()
        {
            var h = new TestHarness(onboarded: true);
            await Flow.DriveToMainMenu(h);

            await h.Machine.PushOverlay(GameState.Settings);
            Assert.AreEqual(GameState.Settings, h.Machine.CurrentState);
            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentBaseState, "base is preserved under an overlay");

            await h.Machine.PopOverlay();
            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentState);
            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentBaseState);
        }

        [Test]
        public async Task BaseChange_ClearsOpenOverlays()
        {
            var h = new TestHarness(onboarded: true);
            await Flow.DriveToMainMenu(h);
            await h.Machine.PushOverlay(GameState.Settings);

            await h.Machine.GoTo(GameState.Shutdown);

            Assert.AreEqual(GameState.Shutdown, h.Machine.CurrentState);
            Assert.AreEqual(GameState.Shutdown, h.Machine.CurrentBaseState);
        }

        [Test]
        public async Task TransitionInFlight_RejectsReentrantTransition()
        {
            var h = new TestHarness(onboarded: true);
            await Flow.DriveToMainMenu(h);

            var gate = new TaskCompletionSource<bool>();
            var secondRan = false;

            var first = h.Machine.TransitionTo(GameState.Hub, async _ => await gate.Task);
            Assert.IsTrue(h.Machine.IsTransitioning);

            LogAssert.Expect(LogType.Warning, new Regex("already in flight"));
            await h.Machine.TransitionTo(GameState.Hub, async _ =>
            {
                secondRan = true;
                await Task.CompletedTask;
            });

            Assert.IsFalse(secondRan, "second transition work must not run while one is in flight");

            gate.SetResult(true);
            await first;
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Hub, h.Machine.CurrentBaseState);
        }
    }
}

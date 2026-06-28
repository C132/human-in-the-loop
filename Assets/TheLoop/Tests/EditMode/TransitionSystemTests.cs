using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Xrcadia.Core.StateMachine;
using Xrcadia.Core.Transitions;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-98 acceptance for the async-transition layer. Showing Loading and landing in the
    /// target is covered by <see cref="LoadingFlowTests"/>; rejecting a re-entrant transition is
    /// covered by <see cref="FsmCoreTests"/>. Closed here: a FAILED transition routes to recovery
    /// and never soft-locks, plus the determinate progress model the Loading screen binds to.
    /// </summary>
    public sealed class TransitionSystemTests
    {
        /// <summary>Stand-in for the real error overlay (owned by XRC-99).</summary>
        private sealed class PlaceholderOverlay : GameStateBase
        {
            private readonly GameState _id;
            public PlaceholderOverlay(GameState id) => _id = id;
            public override GameState Id => _id;
        }

        private static Func<ITransitionReporter, Task> Throwing => async _ =>
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        };

        [Test]
        public async Task FailedTransition_RoutesToErrorOverlay_WhenRegistered()
        {
            var h = new TestHarness(onboarded: true);
            h.Machine.Register(new PlaceholderOverlay(GameState.ErrorModal)); // real overlay is XRC-99
            await Flow.DriveToMainMenu(h);

            LogAssert.Expect(LogType.Error, new Regex("Transition work to Hub failed"));
            await h.Machine.TransitionTo(GameState.Hub, Throwing);
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.ErrorModal, h.Machine.CurrentState, "failure routes to the error overlay");
            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentBaseState, "base is preserved, not the failed target");
            Assert.IsFalse(h.Machine.Transitioning, "the transition flag is released after failure");
        }

        [Test]
        public async Task FailedTransition_NeverSoftLocks_AndStillAcceptsTransitions()
        {
            var h = new TestHarness(onboarded: true); // no ErrorModal registered
            await Flow.DriveToMainMenu(h);

            LogAssert.Expect(LogType.Error, new Regex("Transition work to Hub failed"));
            await h.Machine.TransitionTo(GameState.Hub, Throwing);
            await Flow.WaitUntilIdle(h);

            // The Loading overlay was unwound — nothing stuck on top, machine is idle.
            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentState, "Loading overlay popped after failure");
            Assert.IsFalse(h.Machine.Transitioning);

            // Proof it isn't soft-locked: a later valid transition still runs.
            await h.Machine.GoTo(GameState.Shutdown);
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Shutdown, h.Machine.CurrentBaseState);
        }

        [Test]
        public void LoadingProgress_ClampsReportsAndResets()
        {
            var p = new LoadingProgress();
            var events = new List<(float value, string status)>();
            p.Changed += (v, s) => events.Add((v, s));

            p.Report(0.5f, "Halfway");
            Assert.AreEqual(0.5f, p.Value, 1e-6f);
            Assert.AreEqual("Halfway", p.Status);

            p.Report(2f); // clamps above 1
            Assert.AreEqual(1f, p.Value, 1e-6f);
            Assert.AreEqual("Halfway", p.Status, "a null status leaves the previous line unchanged");

            p.Report(-1f); // clamps below 0
            Assert.AreEqual(0f, p.Value, 1e-6f);

            p.Reset();
            Assert.AreEqual(0f, p.Value, 1e-6f);
            Assert.AreEqual(string.Empty, p.Status);
            Assert.AreEqual(4, events.Count, "every Report/Reset notifies the bound Loading screen");
        }
    }
}

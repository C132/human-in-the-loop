using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TheLoop.App.States;
using Xrcadia.Core.Services;
using Xrcadia.Core.StateMachine;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-99 error &amp; recovery. Recoverable failures pause the current state behind the error
    /// overlay and resume cleanly; fatal failures (a corrupt save) land in the Fatal state and
    /// safe-exit to the Main Menu; no failure dead-ends.
    /// </summary>
    public sealed class ErrorRecoveryTests
    {
        private sealed class PlaceholderState : GameStateBase
        {
            private readonly GameState _id;
            public PlaceholderState(GameState id) => _id = id;
            public override GameState Id => _id;
        }

        private static void RegisterErrorStates(TestHarness h)
        {
            h.Machine.Register(new ErrorModalState());
            h.Machine.Register(new FatalErrorState());
        }

        [Test]
        public async Task RecoverableError_OverlaysCurrentState_AndResumesOnResolve()
        {
            var h = new TestHarness(onboarded: true);
            RegisterErrorStates(h);
            h.Machine.Register(new PlaceholderState(GameState.MRSetup));
            h.Machine.Register(new PlaceholderState(GameState.Session));

            await Flow.DriveToHub(h);
            await h.Machine.Fire(GameTrigger.LaunchRun);   // Hub -> MRSetup
            await h.Machine.GoTo(GameState.Session);
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Session, h.Machine.CurrentBaseState);

            // Tracking lost mid-session.
            await h.Machine.RaiseError(new GameError
            {
                Severity = ErrorSeverity.Recoverable,
                Title = "Tracking lost",
                Message = "Restore headset tracking to continue.",
                Source = "Tracking",
            });
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.ErrorModal, h.Machine.CurrentState, "recoverable error overlays the session");
            Assert.AreEqual(GameState.Session, h.Machine.CurrentBaseState, "session preserved beneath the overlay");

            // Resolved -> resume cleanly.
            await h.Machine.ResumeFromError();
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Session, h.Machine.CurrentState, "resumes the session");
            Assert.IsNull(h.Machine.Context.Error.Current, "error cleared on resume");
        }

        [Test]
        public async Task LoadFailure_RoutesToRecoverableOverlay()
        {
            var h = new TestHarness(onboarded: true);
            RegisterErrorStates(h);
            await Flow.DriveToMainMenu(h);

            LogAssert.Expect(LogType.Error, new Regex("Transition work to Hub failed"));
            await h.Machine.TransitionTo(GameState.Hub, async _ =>
            {
                await Task.Yield();
                throw new RecoverableError("Load", "Load failed", "Content could not be read.");
            });
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.ErrorModal, h.Machine.CurrentState, "a recoverable load failure shows the overlay");
            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentBaseState, "the prior state is preserved");
        }

        [Test]
        public async Task CorruptSave_RoutesToFatal_ThenSafeExitsToMainMenu()
        {
            var h = new TestHarness(onboarded: true);
            RegisterErrorStates(h);
            h.Store.SetString(SaveService.ProfileKey, "}{ corrupt");
            await Flow.DriveToMainMenu(h);

            LogAssert.Expect(LogType.Error, new Regex("Transition work to Hub failed"));
            LogAssert.Expect(LogType.Error, new Regex(@"\[Fatal\] Save"));
            await h.Machine.Fire(GameTrigger.Continue);
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Fatal, h.Machine.CurrentBaseState, "a corrupt save is fatal");

            await h.Machine.SafeExitToMainMenu();
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentBaseState, "fatal safe-exits to the Main Menu");
            Assert.IsNull(h.Machine.Context.Error.Current, "error cleared after safe exit");
        }
    }
}

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Xrcadia.Core.Services;
using Xrcadia.Core.StateMachine;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-92 routing. HasSave gating Continue, and New Game → Hub on a fresh profile, are
    /// covered by <see cref="LoadingFlowTests"/>. Closed here: New Game with an existing save
    /// still lands in Hub (the overwrite path the Main Menu screen confirms), and a corrupted
    /// save on Continue routes to recovery rather than crashing.
    /// </summary>
    public sealed class SaveRoutingTests
    {
        private sealed class PlaceholderOverlay : GameStateBase
        {
            private readonly GameState _id;
            public PlaceholderOverlay(GameState id) => _id = id;
            public override GameState Id => _id;
        }

        [Test]
        public async Task NewGame_WithExistingSave_OverwritesAndEntersHub()
        {
            var h = new TestHarness(onboarded: true, hasSave: true);
            await Flow.DriveToMainMenu(h);

            // Mutate the existing profile so we can prove New Game replaced it.
            h.Save.Profile.currency = 999;
            h.Save.Save();

            await h.Machine.Fire(GameTrigger.NewGame);
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Hub, h.Machine.CurrentBaseState, "New Game lands in Hub");
            Assert.IsTrue(h.Save.HasSave, "a profile still exists after overwrite");
            Assert.AreEqual(0, h.Save.Profile.currency, "New Game created a fresh profile, overwriting the old one");
        }

        [Test]
        public async Task Continue_WithCorruptedSave_RoutesToFatal_NoCrash()
        {
            var h = new TestHarness(onboarded: true); // start with no save
            h.Machine.Register(new PlaceholderOverlay(GameState.Fatal)); // corrupt save is fatal (XRC-99)
            h.Store.SetString(SaveService.ProfileKey, "}{ corrupt"); // a record exists, but unreadable
            await Flow.DriveToMainMenu(h);

            Assert.IsTrue(h.Save.HasSave, "the corrupt record still presents as a save to continue");

            LogAssert.Expect(LogType.Error, new Regex("Transition work to Hub failed"));
            await h.Machine.Fire(GameTrigger.Continue);
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Fatal, h.Machine.CurrentBaseState, "a corrupt save is fatal, never a crash");
            Assert.IsFalse(h.Machine.Transitioning);
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Xrcadia.Core.StateMachine;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-87 acceptance: a headless test drives the full base path
    /// Boot → … → MainMenu → Hub → MRSetup → Session → Results → Hub with no scene-specific
    /// code, and <see cref="GameStateManager.StateChanged"/> fires for every transition.
    ///
    /// MRSetup/Session/Results have no owning state yet (XRC-94/95/97), so behaviorless
    /// placeholders stand in here — this proves the <em>machine</em> drives the complete
    /// diagram, which is XRC-87's scope; per-state behaviour is explicitly out of scope.
    /// </summary>
    public sealed class FsmFullPathTests
    {
        /// <summary>A behaviorless base state standing in for one not yet owned by its issue.</summary>
        private sealed class PlaceholderState : GameStateBase
        {
            private readonly GameState _id;
            public PlaceholderState(GameState id) => _id = id;
            public override GameState Id => _id;
        }

        [Test]
        public async Task FullBasePath_BootToResultsToHub_DrivenHeadlessly()
        {
            var h = new TestHarness(onboarded: true, hasSave: true);

            // Stand-ins for the downstream base states (their edges already exist in the table).
            h.Machine.Register(new PlaceholderState(GameState.MRSetup));
            h.Machine.Register(new PlaceholderState(GameState.Session));
            h.Machine.Register(new PlaceholderState(GameState.Results));

            // The StateChanged event the UI/audio/analytics layers subscribe to must fire.
            var changed = new List<GameState>();
            h.Machine.StateChanged += (from, to) => changed.Add(to);

            // Boot → Splash → Title → MainMenu, then Continue → Hub (save exists).
            await Flow.DriveToMainMenu(h);
            Assert.AreEqual(GameState.MainMenu, h.Machine.CurrentBaseState);

            await h.Machine.Fire(GameTrigger.Continue);
            await Flow.WaitUntilIdle(h);
            Assert.AreEqual(GameState.Hub, h.Machine.CurrentBaseState);

            // Hub → MRSetup → Session → Results → Hub (the run round-trip).
            await h.Machine.GoTo(GameState.MRSetup);
            await h.Machine.GoTo(GameState.Session);
            await h.Machine.GoTo(GameState.Results);
            await h.Machine.GoTo(GameState.Hub);
            await Flow.WaitUntilIdle(h);

            Assert.AreEqual(GameState.Hub, h.Machine.CurrentBaseState);

            // The ordered sequence of base-state replacements matches the diagram's spine.
            var basePath = h.Changes
                .FindAll(c => c.Kind == StateChangeKind.Replace)
                .ConvertAll(c => c.To);
            CollectionAssert.AreEqual(
                new[]
                {
                    GameState.Boot, GameState.Splash, GameState.Title, GameState.MainMenu,
                    GameState.Hub, GameState.MRSetup, GameState.Session, GameState.Results, GameState.Hub,
                },
                basePath);

            // StateChanged observed the run round-trip (proves the event, not just the stack).
            CollectionAssert.IsSubsetOf(
                new[] { GameState.MRSetup, GameState.Session, GameState.Results }, changed);
        }
    }
}

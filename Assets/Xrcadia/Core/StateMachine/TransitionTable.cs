using System.Collections.Generic;

namespace Xrcadia.Core.StateMachine
{
    /// <summary>
    /// The single authored source of legal navigation (XRC-87: "transition table authored
    /// in one place, not scattered conditionals"). Holds the allowed base→base edges and
    /// the trigger→target resolution per base state. Business routing that genuinely
    /// depends on runtime data (e.g. Title → Onboarding vs MainMenu on first launch) stays
    /// in the owning state, but every edge it can take is still declared legal here.
    /// </summary>
    public sealed class TransitionTable
    {
        private readonly HashSet<(GameState from, GameState to)> _edges = new HashSet<(GameState, GameState)>();
        private readonly Dictionary<(GameState from, GameTrigger trigger), GameState> _triggers
            = new Dictionary<(GameState, GameTrigger), GameState>();

        public static TransitionTable BuildDefault()
        {
            var t = new TransitionTable();

            // --- Base flow (mirrors the FSM diagram in XRC-87) ---
            t.Allow(GameState.None, GameState.Boot);        // cold-boot entry
            t.Allow(GameState.Boot, GameState.Splash);
            t.Allow(GameState.Splash, GameState.Title);
            t.Allow(GameState.Title, GameState.Onboarding);
            t.Allow(GameState.Title, GameState.MainMenu);
            t.Allow(GameState.Onboarding, GameState.MainMenu);
            t.Allow(GameState.MainMenu, GameState.Hub);
            t.Allow(GameState.MainMenu, GameState.Shutdown);
            t.Allow(GameState.Hub, GameState.MainMenu);     // exit to menu (XRC-93)
            t.Allow(GameState.Fatal, GameState.MainMenu);   // fatal-error safe exit (XRC-99)

            // Downstream edges declared for completeness so later issues plug in without
            // editing the manager. They have no states registered yet.
            t.Allow(GameState.Hub, GameState.MRSetup);
            t.Allow(GameState.MRSetup, GameState.Session);
            t.Allow(GameState.MRSetup, GameState.Hub);      // cancel setup (XRC-94)
            t.Allow(GameState.Session, GameState.Results);
            t.Allow(GameState.Results, GameState.Hub);

            // --- Trigger resolution ---
            t.OnTrigger(GameState.Onboarding, GameTrigger.OnboardingComplete, GameState.MainMenu);
            t.OnTrigger(GameState.MainMenu, GameTrigger.Continue, GameState.Hub);
            t.OnTrigger(GameState.MainMenu, GameTrigger.NewGame, GameState.Hub);
            t.OnTrigger(GameState.MainMenu, GameTrigger.Quit, GameState.Shutdown);
            t.OnTrigger(GameState.Hub, GameTrigger.LaunchRun, GameState.MRSetup);
            t.OnTrigger(GameState.Hub, GameTrigger.ExitToMenu, GameState.MainMenu);
            // Settings is an overlay (push/pop) — handled by the overlay API, not as a base edge.

            return t;
        }

        public void Allow(GameState from, GameState to) => _edges.Add((from, to));

        public void OnTrigger(GameState from, GameTrigger trigger, GameState target)
            => _triggers[(from, trigger)] = target;

        public bool IsAllowed(GameState from, GameState to) => _edges.Contains((from, to));

        public bool TryResolve(GameState from, GameTrigger trigger, out GameState target)
            => _triggers.TryGetValue((from, trigger), out target);
    }
}

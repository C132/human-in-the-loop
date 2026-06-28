using System.Threading.Tasks;
using Xrcadia.Core.StateMachine;

namespace TheLoop.App.States
{
    /// <summary>
    /// Studio/engine branding. Skippable after a minimum dwell; auto-advances to Title at the
    /// end of its run. The primary-action trigger (StartPressed) requests a skip.
    /// </summary>
    public sealed class SplashState : GameStateBase, ITriggerHandler
    {
        private readonly float _minDwell;
        private readonly float _autoAdvance;

        private float _elapsed;
        private bool _skipRequested;
        private bool _leaving;

        public SplashState(float minDwellSeconds = 1.5f, float autoAdvanceSeconds = 3.5f)
        {
            _minDwell = minDwellSeconds;
            _autoAdvance = autoAdvanceSeconds;
        }

        public override GameState Id => GameState.Splash;

        /// <summary>Seconds elapsed in splash; the screen reads this to gate the skip hint.</summary>
        public float Elapsed => _elapsed;
        public bool CanSkip => _elapsed >= _minDwell;

        public override Task Enter(StateContext context)
        {
            base.Enter(context);
            _elapsed = 0f;
            _skipRequested = false;
            _leaving = false;
            return Task.CompletedTask;
        }

        public override void Tick(float deltaTime)
        {
            _elapsed += deltaTime;

            var done = _elapsed >= _autoAdvance || (_skipRequested && CanSkip);
            if (done && !_leaving)
            {
                _leaving = true;
                Context.Machine.GoTo(GameState.Title).Forget();
            }
        }

        public bool TryHandleTrigger(GameTrigger trigger)
        {
            if (trigger == GameTrigger.StartPressed)
            {
                _skipRequested = true;
                return true;
            }

            return false;
        }
    }
}

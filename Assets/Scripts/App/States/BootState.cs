using System.Threading.Tasks;
using Xrcadia.Core.StateMachine;

namespace Xrcadia.App.States
{
    /// <summary>
    /// Cold-boot entry. Initializes core services (save, settings, audio, XR) behind the
    /// Loading overlay, then advances to Splash. No gameplay assets are loaded here.
    /// The init transition is kicked off from the first Tick (not Enter) so the machine is
    /// never asked to exit Boot while still entering it.
    /// </summary>
    public sealed class BootState : GameStateBase
    {
        bool _started;

        public override GameState Id => GameState.Boot;

        public override Task Enter(StateContext context)
        {
            base.Enter(context);
            _started = false;
            return Task.CompletedTask;
        }

        public override void Tick(float deltaTime)
        {
            if (_started)
            {
                return;
            }

            _started = true;
            Context.Machine.TransitionTo(GameState.Splash, reporter =>
                Context.Services.InitializeAll(reporter)).Forget();
        }
    }
}

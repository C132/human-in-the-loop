using System.Threading.Tasks;

namespace Xrcadia.Core.StateMachine
{
    /// <summary>
    /// A single node in the FSM. Base states model the app's screen/phase; overlay states
    /// additionally use <see cref="OnPause"/>/<see cref="OnResume"/> when something pushes
    /// above / pops back to them.
    /// </summary>
    public interface IGameState
    {
        GameState Id { get; }

        /// <summary>Async so a state may await content/service work as it becomes active.</summary>
        Task Enter(StateContext context);

        Task Exit();

        /// <summary>Pumped each frame while this state is the active (top-of-stack) state.</summary>
        void Tick(float deltaTime);

        /// <summary>Called when an overlay is pushed above this state.</summary>
        void OnPause();

        /// <summary>Called when the overlay above this state is popped.</summary>
        void OnResume();
    }

    /// <summary>Convenience base with no-op defaults so states only override what they need.</summary>
    public abstract class GameStateBase : IGameState
    {
        public abstract GameState Id { get; }

        protected StateContext Context { get; private set; }

        public virtual Task Enter(StateContext context)
        {
            Context = context;
            return Task.CompletedTask;
        }

        public virtual Task Exit() => Task.CompletedTask;

        public virtual void Tick(float deltaTime) { }

        public virtual void OnPause() { }

        public virtual void OnResume() { }
    }
}

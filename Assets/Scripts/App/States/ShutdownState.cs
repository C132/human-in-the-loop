using System;
using System.Threading.Tasks;
using UnityEngine;
using Xrcadia.Core.StateMachine;

namespace Xrcadia.App.States
{
    /// <summary>
    /// Terminal state. Tears down core services in reverse order then quits the application
    /// (stops play mode in the editor). The quit action is injectable so tests can verify
    /// teardown without exiting the test runner.
    /// </summary>
    public sealed class ShutdownState : GameStateBase
    {
        readonly Action _quit;

        public ShutdownState(Action quit = null)
        {
            _quit = quit ?? Quit;
        }

        public override GameState Id => GameState.Shutdown;

        public override Task Enter(StateContext context)
        {
            base.Enter(context);

            context.Services.ShutdownAll();
            _quit();
            return Task.CompletedTask;
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

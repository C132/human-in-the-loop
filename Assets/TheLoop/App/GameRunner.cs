using UnityEngine;
using Xrcadia.Core.Services;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App
{
    /// <summary>
    /// The persistent bootstrap object. Holds the FSM, services and UI router, pumps the
    /// machine each frame, and survives scene loads (DontDestroyOnLoad). Created by
    /// <see cref="Bootstrap"/> at startup — there is no manual scene wiring.
    /// </summary>
    public sealed class GameRunner : MonoBehaviour
    {
        GameStateManager _machine;
        ServiceRegistry _services;
        UIRouter _router;
        bool _servicesDown;

        public GameStateManager Machine => _machine;

        public void Initialize(GameStateManager machine, ServiceRegistry services, UIRouter router)
        {
            _machine = machine;
            _services = services;
            _router = router;
        }

        private void Update()
        {
            _machine?.Tick(Time.deltaTime);
        }

        private void OnApplicationQuit()
        {
            // Safety net if the app is closed without routing through Shutdown.
            if (!_servicesDown && _services != null)
            {
                _servicesDown = true;
                _services.ShutdownAll();
            }

            _router?.Unbind();
        }
    }
}

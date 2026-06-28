using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xrcadia.Core.Transitions;

namespace Xrcadia.Core.Services
{
    /// <summary>
    /// Holds the core services and drives their ordered Boot init / reverse-order Shutdown.
    /// Resolution is by concrete type to keep call sites explicit.
    /// </summary>
    public sealed class ServiceRegistry
    {
        readonly Dictionary<Type, IAppService> _byType = new Dictionary<Type, IAppService>();
        readonly List<IAppService> _ordered = new List<IAppService>();

        public void Register<T>(T service) where T : class, IAppService
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _byType[typeof(T)] = service;
            _ordered.Add(service);
        }

        public T Get<T>() where T : class, IAppService => (T)_byType[typeof(T)];

        public bool TryGet<T>(out T service) where T : class, IAppService
        {
            if (_byType.TryGetValue(typeof(T), out var s))
            {
                service = (T)s;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>Initialize every service in registration order, reporting progress.</summary>
        public async Task InitializeAll(ITransitionReporter reporter = null)
        {
            for (int i = 0; i < _ordered.Count; i++)
            {
                var svc = _ordered[i];
                reporter?.Report((float)i / _ordered.Count, $"Initializing {svc.Name}…");
                await svc.Initialize();
            }

            reporter?.Report(1f, "Ready");
        }

        /// <summary>Shut services down in reverse order so dependents stop before dependencies.</summary>
        public void ShutdownAll()
        {
            for (int i = _ordered.Count - 1; i >= 0; i--)
            {
                try
                {
                    _ordered[i].Shutdown();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[ServiceRegistry] {_ordered[i].Name} failed to shut down: {ex}");
                }
            }
        }
    }
}

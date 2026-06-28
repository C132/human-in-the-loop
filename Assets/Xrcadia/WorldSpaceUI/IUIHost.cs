using System;
using UnityEngine.UIElements;

namespace Xrcadia.UI
{
    /// <summary>
    /// Abstracts the world-space UI surface the router draws into. Keeping the router behind
    /// this interface lets tests run with a fake host and keeps the FSM/router free of
    /// MonoBehaviour lifetime concerns. The panel root is built asynchronously by the underlying
    /// PanelRenderer, so consumers attach through <see cref="RootChanged"/> rather than assuming
    /// <see cref="Root"/> is ready immediately.
    /// </summary>
    public interface IUIHost
    {
        /// <summary>The root element every screen is parented under, or null until first built.</summary>
        VisualElement Root { get; }

        /// <summary>Raised whenever the panel root is (re)built — on load and on live reload.</summary>
        event Action<VisualElement> RootChanged;
    }
}

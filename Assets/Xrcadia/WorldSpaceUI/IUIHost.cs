using UnityEngine.UIElements;

namespace Xrcadia.UI
{
    /// <summary>
    /// Abstracts the world-space UI surface the router draws into. Keeping the router
    /// behind this interface lets tests run with a fake host and keeps the FSM/router free
    /// of MonoBehaviour lifetime concerns.
    /// </summary>
    public interface IUIHost
    {
        /// <summary>The root element every screen is parented under.</summary>
        VisualElement Root { get; }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Devinw.Playerprefmanager.Editor
{
    /// <summary>
    /// Entry point for discovering PlayerPref keys. <see cref="Create"/> returns the
    /// implementation that matches the editor's platform.
    /// </summary>
    public static class PlayerPrefStore
    {
        /// <summary>
        /// Creates the key store for the platform the editor is currently running on.
        /// </summary>
        public static IPlayerPrefStore Create()
        {
#if UNITY_EDITOR_OSX
            return new MacPlayerPrefStore();
#elif UNITY_EDITOR_WIN
            return new WindowsPlayerPrefStore();
#else
            return new UnsupportedPlayerPrefStore();
#endif
        }
    }

    /// <summary>
    /// Fallback store for platforms without a reader. Keys cannot be discovered, but
    /// the editor window can still create and edit keys it already knows about.
    /// </summary>
    internal sealed class UnsupportedPlayerPrefStore : IPlayerPrefStore
    {
        public string Location => $"Key discovery is not supported on {Application.platform}.";

        public IReadOnlyList<PlayerPrefEntry> LoadKeys()
        {
            return new List<PlayerPrefEntry>();
        }
    }
}

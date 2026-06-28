using System.Collections.Generic;

namespace XRCADIA.PrefHub.Editor
{
    /// <summary>
    /// Discovers the set of PlayerPref keys for the current project.
    ///
    /// Unity's <see cref="UnityEngine.PlayerPrefs"/> API can read and write a key but
    /// cannot enumerate keys, so each platform implementation reads the underlying
    /// store (macOS preference domain, Windows registry, etc.) to list them.
    /// </summary>
    public interface IPlayerPrefStore
    {
        /// <summary>
        /// A human-readable description of where prefs are read from, shown in the UI.
        /// </summary>
        string Location { get; }

        /// <summary>
        /// Reads every PlayerPref key and its stored type from the platform store.
        /// </summary>
        IReadOnlyList<PlayerPrefEntry> LoadKeys();
    }
}

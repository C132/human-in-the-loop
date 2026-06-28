#if UNITY_EDITOR_WIN
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using UnityEngine;

namespace XRCADIA.PrefHub.Editor
{
    /// <summary>
    /// Reads PlayerPref keys on Windows, where Unity stores them under
    /// <c>HKCU\Software\[company]\[product]</c>. Each value name is the key with a
    /// <c>_h[hash]</c> suffix appended by Unity, which is stripped here.
    ///
    /// Strings are stored as binary, ints as DWORD and floats as QWORD. Unity reuses
    /// these registry kinds, so the mapping below is a best effort: an int and a float
    /// that happen to share a representation cannot always be told apart.
    /// </summary>
    internal sealed class WindowsPlayerPrefStore : IPlayerPrefStore
    {
        private static readonly Regex HashSuffix = new Regex("_h\\d+$");

        private readonly string _registryPath;

        public WindowsPlayerPrefStore()
        {
            _registryPath = $"Software\\{Application.companyName}\\{Application.productName}";
        }

        public string Location => $"HKCU\\{_registryPath}";

        public IReadOnlyList<PlayerPrefEntry> LoadKeys()
        {
            var entries = new List<PlayerPrefEntry>();

            using var key = Registry.CurrentUser.OpenSubKey(_registryPath);
            if (key == null)
                return entries;

            foreach (var valueName in key.GetValueNames())
            {
                var prefKey = HashSuffix.Replace(valueName, string.Empty);
                var type = MapType(key.GetValueKind(valueName));
                entries.Add(new PlayerPrefEntry(prefKey, type));
            }

            return entries;
        }

        private static PlayerPrefType MapType(RegistryValueKind kind)
        {
            switch (kind)
            {
                case RegistryValueKind.Binary:
                    return PlayerPrefType.String;
                case RegistryValueKind.QWord:
                    return PlayerPrefType.Float;
                default:
                    return PlayerPrefType.Int;
            }
        }
    }
}
#endif

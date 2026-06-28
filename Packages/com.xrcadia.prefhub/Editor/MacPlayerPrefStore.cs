#if UNITY_EDITOR_OSX
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace XRCADIA.PrefHub.Editor
{
    /// <summary>
    /// Reads PlayerPref keys on macOS, where Unity stores them in the
    /// <c>unity.[company].[product]</c> CoreFoundation preference domain.
    ///
    /// The <c>defaults export</c> command reads the live value through cfprefsd,
    /// avoiding the stale on-disk plist, and emits an XML plist we can parse.
    /// </summary>
    internal sealed class MacPlayerPrefStore : IPlayerPrefStore
    {
        private readonly string _domain;

        public MacPlayerPrefStore()
        {
            _domain = $"unity.{Application.companyName}.{Application.productName}";
        }

        public string Location => $"~/Library/Preferences/{_domain}.plist";

        public IReadOnlyList<PlayerPrefEntry> LoadKeys()
        {
            var entries = new List<PlayerPrefEntry>();

            var xml = ExportDomain();
            if (string.IsNullOrEmpty(xml))
                return entries;

            var document = new XmlDocument();
            try
            {
                document.LoadXml(xml);
            }
            catch (XmlException exception)
            {
                Debug.LogWarning($"PrefHub: could not parse preferences for {_domain}. {exception.Message}");
                return entries;
            }

            var dictionary = document.SelectSingleNode("/plist/dict");
            if (dictionary == null)
                return entries;

            var children = dictionary.ChildNodes;
            for (int i = 0; i + 1 < children.Count; i++)
            {
                var keyNode = children[i];
                if (keyNode.Name != "key")
                    continue;

                var valueNode = children[i + 1];
                if (TryMapType(valueNode.Name, out var type))
                    entries.Add(new PlayerPrefEntry(keyNode.InnerText, type));
            }

            return entries;
        }

        private string ExportDomain()
        {
            var info = new ProcessStartInfo
            {
                FileName = "/usr/bin/defaults",
                Arguments = $"export \"{_domain}\" -",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using var process = Process.Start(info);
                if (process == null)
                    return null;

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"PrefHub: failed to read preferences. {exception.Message}");
                return null;
            }
        }

        private static bool TryMapType(string plistElement, out PlayerPrefType type)
        {
            switch (plistElement)
            {
                case "integer":
                    type = PlayerPrefType.Int;
                    return true;
                case "real":
                    type = PlayerPrefType.Float;
                    return true;
                case "string":
                    type = PlayerPrefType.String;
                    return true;
                default:
                    type = default;
                    return false;
            }
        }
    }
}
#endif

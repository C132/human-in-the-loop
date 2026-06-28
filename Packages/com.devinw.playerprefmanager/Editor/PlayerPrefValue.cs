using System.Globalization;
using UnityEngine;

namespace Devinw.Playerprefmanager.Editor
{
    /// <summary>
    /// Reads and writes PlayerPref values as strings, handling type parsing and
    /// culture-invariant formatting. Separated from the editor window so the
    /// conversion logic can be unit tested.
    /// </summary>
    internal static class PlayerPrefValue
    {
        public static string Read(PlayerPrefEntry entry)
        {
            switch (entry.Type)
            {
                case PlayerPrefType.Int:
                    return PlayerPrefs.GetInt(entry.Key).ToString(CultureInfo.InvariantCulture);
                case PlayerPrefType.Float:
                    return PlayerPrefs.GetFloat(entry.Key).ToString(CultureInfo.InvariantCulture);
                default:
                    return PlayerPrefs.GetString(entry.Key);
            }
        }

        public static bool TryWrite(string key, PlayerPrefType type, string rawValue)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            switch (type)
            {
                case PlayerPrefType.Int:
                    if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
                        return false;
                    PlayerPrefs.SetInt(key, intValue);
                    break;

                case PlayerPrefType.Float:
                    if (!float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue))
                        return false;
                    PlayerPrefs.SetFloat(key, floatValue);
                    break;

                case PlayerPrefType.String:
                    PlayerPrefs.SetString(key, rawValue ?? string.Empty);
                    break;
            }

            PlayerPrefs.Save();
            return true;
        }
    }
}

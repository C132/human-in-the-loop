namespace XRCADIA.PrefHub.Editor
{
    /// <summary>
    /// The value type a <see cref="UnityEngine.PlayerPrefs"/> entry is stored as.
    /// </summary>
    public enum PlayerPrefType
    {
        Int,
        Float,
        String
    }

    /// <summary>
    /// A single discovered PlayerPref: the key and the type it is stored as.
    /// The live value is read separately through <see cref="UnityEngine.PlayerPrefs"/>.
    /// </summary>
    public readonly struct PlayerPrefEntry
    {
        public string Key { get; }
        public PlayerPrefType Type { get; }

        public PlayerPrefEntry(string key, PlayerPrefType type)
        {
            Key = key;
            Type = type;
        }
    }
}

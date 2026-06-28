namespace Xrcadia.Core.Services
{
    /// <summary>
    /// Tiny persistence abstraction so services (save flag, first-launch flag, settings)
    /// stay engine-free and unit-testable. The runtime uses a PlayerPrefs-backed store;
    /// tests use <see cref="InMemoryKeyValueStore"/>.
    /// </summary>
    public interface IKeyValueStore
    {
        bool HasKey(string key);
        int GetInt(string key, int defaultValue = 0);
        void SetInt(string key, int value);
        string GetString(string key, string defaultValue = "");
        void SetString(string key, string value);
        void DeleteKey(string key);
        void Save();
    }
}

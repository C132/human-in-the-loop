using System.Collections.Generic;

namespace Xrcadia.Core.Services
{
    /// <summary>Non-persistent store for tests and headless runs.</summary>
    public sealed class InMemoryKeyValueStore : IKeyValueStore
    {
        readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public bool HasKey(string key) => _values.ContainsKey(key);

        public int GetInt(string key, int defaultValue = 0)
            => _values.TryGetValue(key, out var v) && v is int i ? i : defaultValue;

        public void SetInt(string key, int value) => _values[key] = value;

        public string GetString(string key, string defaultValue = "")
            => _values.TryGetValue(key, out var v) && v is string s ? s : defaultValue;

        public void SetString(string key, string value) => _values[key] = value;

        public void DeleteKey(string key) => _values.Remove(key);

        public void Save() { /* no-op: already in memory */ }
    }
}

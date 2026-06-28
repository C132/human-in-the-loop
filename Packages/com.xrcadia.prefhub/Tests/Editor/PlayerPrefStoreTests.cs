using System.Linq;
using NUnit.Framework;
using UnityEngine;
using XRCADIA.PrefHub.Editor;

namespace XRCADIA.PrefHub.EditorTests
{
    public sealed class PlayerPrefStoreTests
    {
        private const string TestKey = "ppm_store_test_key";

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(TestKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void Create_ReturnsStoreWithLocation()
        {
            var store = PlayerPrefStore.Create();

            Assert.IsNotNull(store);
            Assert.IsFalse(string.IsNullOrEmpty(store.Location));
        }

        [Test]
        public void LoadKeys_DiscoversWrittenKeyWithCorrectType()
        {
            PlayerPrefs.SetInt(TestKey, 7);
            PlayerPrefs.Save();

            var store = PlayerPrefStore.Create();
            var entry = store.LoadKeys().FirstOrDefault(e => e.Key == TestKey);

            Assert.AreEqual(TestKey, entry.Key, "Store did not discover the written key.");
            Assert.AreEqual(PlayerPrefType.Int, entry.Type);
        }

        [Test]
        public void LoadKeys_OmitsDeletedKey()
        {
            PlayerPrefs.SetInt(TestKey, 7);
            PlayerPrefs.Save();
            PlayerPrefs.DeleteKey(TestKey);
            PlayerPrefs.Save();

            var store = PlayerPrefStore.Create();

            Assert.IsFalse(store.LoadKeys().Any(e => e.Key == TestKey));
        }
    }
}

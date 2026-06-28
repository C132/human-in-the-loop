using System.Threading.Tasks;
using NUnit.Framework;
using Xrcadia.Core.Services;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-92 save/profile service: create, round-trip across a restart, and corruption
    /// tolerance (unparseable or unsupported-version data throws rather than returning junk).
    /// </summary>
    public sealed class SaveServiceTests
    {
        [Test]
        public async Task CreateNew_MakesCurrentVersionProfile_AndGatesContinue()
        {
            var save = new SaveService(new InMemoryKeyValueStore());
            Assert.IsFalse(save.HasSave);

            await save.CreateNew();

            Assert.IsTrue(save.HasSave);
            Assert.IsNotNull(save.Profile);
            Assert.AreEqual(SaveService.CurrentVersion, save.Profile.version);
            Assert.AreEqual(1, save.Profile.labLevel);
        }

        [Test]
        public async Task Load_RoundTripsAcrossRestart()
        {
            var store = new InMemoryKeyValueStore();

            var before = new SaveService(store);
            await before.CreateNew();
            before.Profile.currency = 42;
            before.Save();

            var after = new SaveService(store); // a new instance over the same store = a restart
            await after.Load();

            Assert.AreEqual(42, after.Profile.currency);
            Assert.AreEqual(SaveService.CurrentVersion, after.Profile.version);
        }

        [Test]
        public void Load_OnUnparseableData_ThrowsCorruption()
        {
            var store = new InMemoryKeyValueStore();
            store.SetString(SaveService.ProfileKey, "}{ not json");
            var save = new SaveService(store);

            Assert.IsTrue(save.HasSave, "a record exists; validity is only checked on load");
            Assert.ThrowsAsync<SaveCorruptionException>(async () => await save.Load());
        }

        [Test]
        public void Load_OnUnsupportedVersion_ThrowsCorruption()
        {
            var store = new InMemoryKeyValueStore();
            store.SetString(SaveService.ProfileKey, "{\"version\":999}");
            var save = new SaveService(store);

            Assert.ThrowsAsync<SaveCorruptionException>(async () => await save.Load());
        }

        [Test]
        public async Task Delete_ClearsHasSave()
        {
            var save = new SaveService(new InMemoryKeyValueStore());
            await save.CreateNew();
            Assert.IsTrue(save.HasSave);

            save.Delete();
            Assert.IsFalse(save.HasSave);
        }
    }
}

using System.Threading.Tasks;
using NUnit.Framework;
using Xrcadia.Core.Services;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-91 acceptance: settings changes persist across an app restart. The overlay's
    /// push/pop + base-restore behaviour is base-agnostic and already proven — generically by
    /// <see cref="FsmCoreTests"/> (Overlay_PushAndPop_RestoresBase) and over Main Menu by
    /// <see cref="MainMenuTests"/>; Hub/Pause entry points arrive with XRC-93/XRC-96. A
    /// "restart" here is a fresh <see cref="SettingsService"/> re-reading the same store.
    /// </summary>
    public sealed class SettingsServiceTests
    {
        [Test]
        public async Task Preferences_PersistAcrossRestart()
        {
            var store = new InMemoryKeyValueStore();

            var before = new SettingsService(store);
            before.SetMasterVolume(0.3f);
            before.SetMusicVolume(0.6f);
            before.SetComfortVignette(false);
            before.SetSnapTurn(false);
            before.SetSubtitles(true);

            // Restart: a new service instance over the same persisted store.
            var after = new SettingsService(store);
            await after.Initialize();

            Assert.AreEqual(0.3f, after.MasterVolume, 1e-2f);
            Assert.AreEqual(0.6f, after.MusicVolume, 1e-2f);
            Assert.IsFalse(after.ComfortVignette);
            Assert.IsFalse(after.SnapTurn);
            Assert.IsTrue(after.Subtitles);
        }

        [Test]
        public async Task Defaults_AreComfortFirst_OnFreshProfile()
        {
            var after = new SettingsService(new InMemoryKeyValueStore());
            await after.Initialize();

            Assert.AreEqual(1f, after.MasterVolume, 1e-2f);
            Assert.IsTrue(after.ComfortVignette, "comfort vignette defaults on");
            Assert.IsTrue(after.SnapTurn, "snap turn is the comfort default");
            Assert.IsFalse(after.Subtitles);
        }

        [Test]
        public void Volume_ClampsToUnitRange()
        {
            var s = new SettingsService(new InMemoryKeyValueStore());
            s.SetMasterVolume(5f);
            Assert.AreEqual(1f, s.MasterVolume, 1e-6f);
            s.SetMasterVolume(-2f);
            Assert.AreEqual(0f, s.MasterVolume, 1e-6f);
        }
    }
}

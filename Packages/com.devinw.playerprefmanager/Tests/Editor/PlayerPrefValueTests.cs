using NUnit.Framework;
using UnityEngine;

namespace Devinw.Playerprefmanager.Editor.Tests
{
    public sealed class PlayerPrefValueTests
    {
        private const string TestKey = "ppm_value_test_key";

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(TestKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void TryWrite_Int_RoundTripsThroughRead()
        {
            Assert.IsTrue(PlayerPrefValue.TryWrite(TestKey, PlayerPrefType.Int, "42"));
            Assert.AreEqual("42", PlayerPrefValue.Read(new PlayerPrefEntry(TestKey, PlayerPrefType.Int)));
        }

        [Test]
        public void TryWrite_Float_RoundTripsWithInvariantCulture()
        {
            Assert.IsTrue(PlayerPrefValue.TryWrite(TestKey, PlayerPrefType.Float, "3.5"));
            Assert.AreEqual("3.5", PlayerPrefValue.Read(new PlayerPrefEntry(TestKey, PlayerPrefType.Float)));
        }

        [Test]
        public void TryWrite_String_RoundTripsThroughRead()
        {
            Assert.IsTrue(PlayerPrefValue.TryWrite(TestKey, PlayerPrefType.String, "hello world"));
            Assert.AreEqual("hello world", PlayerPrefValue.Read(new PlayerPrefEntry(TestKey, PlayerPrefType.String)));
        }

        [Test]
        public void TryWrite_NonNumericInt_ReturnsFalseAndDoesNotWrite()
        {
            Assert.IsFalse(PlayerPrefValue.TryWrite(TestKey, PlayerPrefType.Int, "not-a-number"));
            Assert.IsFalse(PlayerPrefs.HasKey(TestKey));
        }

        [Test]
        public void TryWrite_NonNumericFloat_ReturnsFalse()
        {
            Assert.IsFalse(PlayerPrefValue.TryWrite(TestKey, PlayerPrefType.Float, "abc"));
        }

        [Test]
        public void TryWrite_EmptyKey_ReturnsFalse()
        {
            Assert.IsFalse(PlayerPrefValue.TryWrite(string.Empty, PlayerPrefType.String, "value"));
        }
    }
}

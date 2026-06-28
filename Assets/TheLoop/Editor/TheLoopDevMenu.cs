using UnityEditor;
using UnityEngine;

namespace TheLoop.Editor
{
    /// <summary>
    /// Editor conveniences for testing the front-end flow. The onboarding/save flags persist in
    /// PlayerPrefs, so once the flow runs once there is no in-editor way to see Onboarding again.
    /// Reset First Launch clears those flags so the next Play boots as a true first launch
    /// (Title → Onboarding, Continue disabled).
    /// </summary>
    internal static class TheLoopDevMenu
    {
        // Mirror of the persistence keys owned by SettingsService / SaveService (Xrcadia.Core).
        private const string OnboardingKey = "hitl.onboarding.completed";
        private const string SaveKey = "hitl.save.profile";

        [MenuItem("TheLoop/Reset First Launch (clear save + onboarding)")]
        private static void ResetFirstLaunch()
        {
            PlayerPrefs.DeleteKey(OnboardingKey);
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
            Debug.Log("[TheLoop] Reset to first launch: cleared onboarding + save. Next Play starts at Onboarding.");
        }
    }
}

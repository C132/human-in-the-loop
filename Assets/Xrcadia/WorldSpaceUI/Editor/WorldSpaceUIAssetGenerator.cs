using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Xrcadia.UI;

namespace Xrcadia.Editor
{
    /// <summary>
    /// One-click creation of the world-space PanelSettings the runtime loads from Resources.
    /// This is the single build-time setup step for the loading flow; the runtime itself needs
    /// no manual scene wiring. Re-running it is safe (it updates the existing asset).
    /// </summary>
    public static class WorldSpaceUIAssetGenerator
    {
        private const string ResourcesDir = "Assets/Resources/UI";
        private const string PanelSettingsPath = ResourcesDir + "/HitLPanelSettings.asset";

        [MenuItem("Xrcadia/UI/Generate World-Space UI Assets")]
        public static void Generate()
        {
            Directory.CreateDirectory(ResourcesDir);

            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            var created = settings == null;
            if (created)
            {
                settings = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(settings, PanelSettingsPath);
            }

            settings.scaleMode = PanelScaleMode.ConstantPhysicalSize;
            AssignDefaultTheme(settings);
            TrySetWorldSpaceRenderMode(settings);

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Xrcadia] {(created ? "Created" : "Updated")} world-space PanelSettings at {PanelSettingsPath}.");
        }

        private static void AssignDefaultTheme(PanelSettings settings)
        {
            if (settings.themeStyleSheet != null)
            {
                return;
            }

            // Find any ThemeStyleSheet in the project (the UI Toolkit default runtime theme,
            // created automatically when the package is present), preferring one named "default".
            var guids = AssetDatabase.FindAssets("t:ThemeStyleSheet");
            ThemeStyleSheet best = null;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var tss = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(path);
                if (tss == null) continue;
                best = tss;
                if (path.ToLowerInvariant().Contains("default")) break;
            }

            if (best != null)
            {
                settings.themeStyleSheet = best;
            }
            else
            {
                Debug.LogWarning("[Xrcadia] No ThemeStyleSheet found. Create one via " +
                    "Assets ▸ Create ▸ UI Toolkit ▸ Theme Style Sheet and assign it to the PanelSettings.");
            }
        }

        /// <summary>
        /// Set Render Mode = World Space via reflection so this compiles regardless of the exact
        /// Unity 6 API surface. If the property isn't present, the panel stays screen-space and a
        /// note is logged (XRC-86 owns the deep world-space rendering setup).
        /// </summary>
        private static void TrySetWorldSpaceRenderMode(PanelSettings settings)
        {
            var prop = typeof(PanelSettings).GetProperty("renderMode",
                BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
            {
                Debug.Log("[Xrcadia] PanelSettings.renderMode not found in this Unity version; " +
                    "set Render Mode to World Space manually if available.");
                return;
            }

            try
            {
                var worldSpace = Enum.Parse(prop.PropertyType, "WorldSpace");
                prop.SetValue(settings, worldSpace);
                Debug.Log("[Xrcadia] PanelSettings render mode set to World Space.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Xrcadia] Could not set World Space render mode automatically: {ex.Message}");
            }
        }
    }
}

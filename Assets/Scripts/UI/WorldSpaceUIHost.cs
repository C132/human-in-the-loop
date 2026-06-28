using UnityEngine;
using UnityEngine.UIElements;

namespace Xrcadia.UI
{
    /// <summary>
    /// Owns the UI Toolkit <see cref="UIDocument"/> the router draws into and renders it in
    /// world space for the MR tabletop (XRC-86). The PanelSettings asset (with Render Mode =
    /// World Space) is authored once via the editor generator and loaded from Resources; if
    /// absent we fall back to a runtime panel so the flow still runs in-editor, with a warning.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class WorldSpaceUIHost : MonoBehaviour, IUIHost
    {
        public const string PanelSettingsResourcePath = "UI/HitLPanelSettings";
        public const string ThemeResourcePath = "UI/HitLTheme";

        // Comfortable default placement: ~1.5 m ahead, near table height. Tunable by XRC-94.
        static readonly Vector3 DefaultWorldPosition = new Vector3(0f, 1.1f, 1.5f);

        UIDocument _document;

        public VisualElement Root => _document != null ? _document.rootVisualElement : null;

        /// <summary>
        /// Create a fully-wired host (used by the runtime bootstrap). The GameObject is built
        /// inactive so the PanelSettings is assigned before <see cref="UIDocument"/> enables and
        /// builds its root — guaranteeing <see cref="Root"/> is ready when screens register.
        /// </summary>
        public static WorldSpaceUIHost Create()
        {
            var go = new GameObject("WorldSpaceUIHost");
            go.SetActive(false);

            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = LoadOrCreatePanelSettings();

            var host = go.AddComponent<WorldSpaceUIHost>();
            host._document = doc;

            go.transform.position = DefaultWorldPosition;
            go.SetActive(true); // OnEnable now builds rootVisualElement with the panel assigned.

            host.ApplyTheme();
            return host;
        }

        void Awake()
        {
            if (_document == null)
            {
                _document = GetComponent<UIDocument>();
            }
        }

        static PanelSettings LoadOrCreatePanelSettings()
        {
            var settings = Resources.Load<PanelSettings>(PanelSettingsResourcePath);
            if (settings != null)
            {
                return settings;
            }

            Debug.LogWarning(
                $"[WorldSpaceUIHost] No PanelSettings at Resources/{PanelSettingsResourcePath}. " +
                "Run 'Xrcadia ▸ UI ▸ Generate World-Space UI Assets' once to create a world-space panel. " +
                "Using a runtime fallback panel for now.");

            var fallback = ScriptableObject.CreateInstance<PanelSettings>();
            fallback.name = "HitLPanelSettings (runtime fallback)";
            fallback.scaleMode = PanelScaleMode.ConstantPhysicalSize;
            return fallback;
        }

        void ApplyTheme()
        {
            var root = Root;
            if (root == null)
            {
                return;
            }

            var theme = Resources.Load<StyleSheet>(ThemeResourcePath);
            if (theme != null && !root.styleSheets.Contains(theme))
            {
                root.styleSheets.Add(theme);
            }

            root.AddToClassList("hitl-root");
            root.style.flexGrow = 1; // router fills it with absolutely-positioned screens.
        }
    }
}

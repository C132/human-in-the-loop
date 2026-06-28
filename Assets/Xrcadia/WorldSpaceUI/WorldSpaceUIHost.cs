using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Xrcadia.UI
{
    /// <summary>
    /// Owns the UI Toolkit panel the router draws into and renders it in world space for the
    /// MR tabletop (XRC-86). Built on <see cref="PanelRenderer"/> — the successor to the
    /// deprecated UIDocument component — so the panel root arrives through a UI-reload callback
    /// instead of a hidden companion. The world-space PanelSettings is authored once via the
    /// editor generator and loaded from Resources; if absent we fall back to a runtime panel so
    /// the flow still runs in-editor, with a warning.
    /// </summary>
    [RequireComponent(typeof(PanelRenderer))]
    public sealed class WorldSpaceUIHost : MonoBehaviour, IUIHost
    {
        public const string PanelSettingsResourcePath = "UI/HitLPanelSettings";
        public const string ThemeResourcePath = "UI/HitLTheme";

        // Comfortable default placement: ~1.5 m ahead, near table height. Tunable by XRC-94.
        private static readonly Vector3 DefaultWorldPosition = new Vector3(0f, 1.1f, 1.5f);

        private PanelRenderer _renderer;
        private VisualElement _root;

        public VisualElement Root => _root;

        public event Action<VisualElement> RootChanged;

        /// <summary>
        /// Create a fully-wired host (used by the runtime bootstrap). The GameObject is built
        /// inactive and the reload callback is registered before enabling, so the panel root is
        /// captured synchronously the moment <see cref="PanelRenderer"/> loads it — guaranteeing
        /// <see cref="Root"/> is ready when screens register.
        /// </summary>
        public static WorldSpaceUIHost Create()
        {
            var go = new GameObject("WorldSpaceUIHost");
            go.SetActive(false);

            var panelRenderer = go.AddComponent<PanelRenderer>();
            panelRenderer.panelSettings = LoadOrCreatePanelSettings();

            var host = go.AddComponent<WorldSpaceUIHost>();
            host._renderer = panelRenderer;
            panelRenderer.RegisterUIReloadCallback(host.OnUIReload);

            go.transform.position = DefaultWorldPosition;
            go.SetActive(true); // Enabling loads the panel, firing OnUIReload with the root.

            return host;
        }

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<PanelRenderer>();
        }

        private void OnDestroy()
        {
            if (_renderer != null)
                _renderer.UnregisterUIReloadCallback(OnUIReload);
        }

        // Fires when PanelRenderer (re)builds its visual tree — on enable and on live reload.
        private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            _root = root;
            ApplyTheme(root);
            RootChanged?.Invoke(root);
        }

        private static PanelSettings LoadOrCreatePanelSettings()
        {
            var settings = Resources.Load<PanelSettings>(PanelSettingsResourcePath);
            if (settings != null)
                return settings;

            Debug.LogWarning(
                $"[WorldSpaceUIHost] No PanelSettings at Resources/{PanelSettingsResourcePath}. " +
                "Run 'Xrcadia ▸ UI ▸ Generate World-Space UI Assets' once to create a world-space panel. " +
                "Using a runtime fallback panel for now.");

            var fallback = ScriptableObject.CreateInstance<PanelSettings>();
            fallback.name = "HitLPanelSettings (runtime fallback)";
            fallback.scaleMode = PanelScaleMode.ConstantPhysicalSize;
            return fallback;
        }

        private static void ApplyTheme(VisualElement root)
        {
            if (root == null)
                return;

            var theme = Resources.Load<StyleSheet>(ThemeResourcePath);
            if (theme != null && !root.styleSheets.Contains(theme))
                root.styleSheets.Add(theme);

            root.AddToClassList("hitl-root");
            root.style.flexGrow = 1; // Router fills it with absolutely-positioned screens.
        }
    }
}

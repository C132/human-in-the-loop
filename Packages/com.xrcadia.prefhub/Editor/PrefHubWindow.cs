using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XRCADIA.PrefHub.Editor
{
    /// <summary>
    /// PrefHub — an editor window for viewing, editing, adding and deleting the
    /// project's <see cref="PlayerPrefs"/>. Keys are discovered through an
    /// <see cref="IPlayerPrefStore"/>; values are read and written through Unity's
    /// PlayerPrefs API.
    /// </summary>
    public sealed class PrefHubWindow : EditorWindow
    {
        private sealed class PrefRow
        {
            public string Key;
            public PlayerPrefType Type;
            public string Value;
        }

        private const float TypeColumnWidth = 70f;
        private const float ValueColumnWidth = 200f;
        private const float DeleteColumnWidth = 24f;

        private static readonly string[] TypeNames = { "Int", "Float", "String" };

        private IPlayerPrefStore _store;
        private List<PrefRow> _rows = new();
        private Vector2 _scroll;
        private string _search = string.Empty;

        private string _newKey = string.Empty;
        private PlayerPrefType _newType = PlayerPrefType.String;
        private string _newValue = string.Empty;

        /// <summary>
        /// Opens (or focuses) the PrefHub window.
        /// </summary>
        [MenuItem("xrcadia/PrefHub/Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<PrefHubWindow>();
            window.titleContent = new GUIContent("PrefHub");
            window.minSize = new Vector2(420f, 240f);
            window.Show();
        }

        private void OnEnable()
        {
            _store = PlayerPrefStore.Create();
            Reload();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawLocation();

            if (_rows.Count == 0)
                EditorGUILayout.HelpBox("No PlayerPrefs found. Add one below.", MessageType.Info);
            else
                DrawRows();

            DrawAddSection();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                    Reload();

                GUILayout.FlexibleSpace();

                _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField, GUILayout.Width(180f));

                if (GUILayout.Button("Delete All", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                    DeleteAll();
            }
        }

        private void DrawLocation()
        {
            if (_store == null)
                return;

            EditorGUILayout.LabelField(_store.Location, EditorStyles.miniLabel);
        }

        private void DrawRows()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (var row in _rows)
            {
                if (!Matches(row.Key))
                    continue;

                DrawRow(row);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawRow(PrefRow row)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.SelectableLabel(row.Key, GUILayout.Height(EditorGUIUtility.singleLineHeight));

                EditorGUI.BeginChangeCheck();
                var type = (PlayerPrefType)EditorGUILayout.Popup((int)row.Type, TypeNames, GUILayout.Width(TypeColumnWidth));
                var value = EditorGUILayout.TextField(row.Value, GUILayout.Width(ValueColumnWidth));
                if (EditorGUI.EndChangeCheck())
                {
                    row.Type = type;
                    row.Value = value;
                    Write(row);
                }

                if (GUILayout.Button("X", GUILayout.Width(DeleteColumnWidth)))
                    Delete(row);
            }
        }

        private void DrawAddSection()
        {
            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Add / Update Key", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _newKey = EditorGUILayout.TextField(_newKey);
                    _newType = (PlayerPrefType)EditorGUILayout.Popup((int)_newType, TypeNames, GUILayout.Width(TypeColumnWidth));
                    _newValue = EditorGUILayout.TextField(_newValue, GUILayout.Width(ValueColumnWidth));

                    using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_newKey)))
                    {
                        if (GUILayout.Button("Add", GUILayout.Width(DeleteColumnWidth + 30f)))
                            Add();
                    }
                }
            }
        }

        private void Add()
        {
            var row = new PrefRow { Key = _newKey, Type = _newType, Value = _newValue };
            if (!Write(row))
                return;

            _newKey = string.Empty;
            _newValue = string.Empty;
            Reload();
        }

        private bool Write(PrefRow row)
        {
            return PlayerPrefValue.TryWrite(row.Key, row.Type, row.Value);
        }

        private void Delete(PrefRow row)
        {
            PlayerPrefs.DeleteKey(row.Key);
            PlayerPrefs.Save();
            _rows.Remove(row);
        }

        private void DeleteAll()
        {
            var confirmed = EditorUtility.DisplayDialog(
                "Delete All PlayerPrefs",
                "This permanently deletes every PlayerPref for this project. This cannot be undone.",
                "Delete All",
                "Cancel");

            if (!confirmed)
                return;

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Reload();
        }

        private void Reload()
        {
            _rows = new List<PrefRow>();

            if (_store == null)
                return;

            foreach (var entry in _store.LoadKeys())
                _rows.Add(new PrefRow { Key = entry.Key, Type = entry.Type, Value = PlayerPrefValue.Read(entry) });
        }

        private bool Matches(string key)
        {
            if (string.IsNullOrEmpty(_search))
                return true;

            return key.IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}

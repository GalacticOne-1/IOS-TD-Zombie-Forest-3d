using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Tools
{
    public class RuntimeIdDatabaseWindow : EditorWindow
    {
        private Vector2 scroll;
        private List<RuntimeIdEntry> entries;
        private Dictionary<string, List<RuntimeIdEntry>> grouped;

        private bool showOnlyDuplicates;

        [MenuItem("Tools/Database/RuntimeId Inspector")]
        public static void Open()
        {
            GetWindow<RuntimeIdDatabaseWindow>("RuntimeId DB");
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            entries = RuntimeIdDatabaseScanner.Scan(out grouped);
        }

        private void OnGUI()
        {
            DrawToolbar();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            if (entries == null)
            {
                EditorGUILayout.HelpBox("No data loaded", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            foreach (var e in entries)
            {
                if (showOnlyDuplicates && !e.IsDuplicate)
                    continue;

                DrawEntry(e);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                Refresh();

            showOnlyDuplicates =
                GUILayout.Toggle(showOnlyDuplicates, "Only Duplicates", EditorStyles.toolbarButton);

            if (GUILayout.Button("Fix Duplicates", EditorStyles.toolbarButton))
                FixDuplicates();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawEntry(RuntimeIdEntry e)
        {
            GUI.color = e.IsDuplicate ? Color.red : Color.white;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.ObjectField(e.RuntimeId, typeof(ScriptableObject), false);

            if (GUILayout.Button("Ping", GUILayout.Width(60)))
                EditorGUIUtility.PingObject(e.RuntimeId);

            if (GUILayout.Button("Select", GUILayout.Width(60)))
                Selection.activeObject = e.RuntimeId;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"GUID: {e.Guid}");
            EditorGUILayout.LabelField(e.Path);

            EditorGUILayout.EndVertical();

            GUI.color = Color.white;
        }

        private void FixDuplicates()
        {
            if (entries == null) return;

            Undo.RecordObjects(
                entries.Select(e => e.RuntimeId).ToArray(),
                "Fix RuntimeId Duplicates"
            );

            GConsole.ClearLog();
            
            
            var seen = new HashSet<string>();
            var ordered = entries
                .OrderBy(e => e.Path)
                .ThenBy(e => e.Guid)
                .ToList();

            foreach (var e in ordered)
            {
                if (string.IsNullOrEmpty(e.Guid))
                    continue;

                if (!seen.Contains(e.Guid))
                {
                    seen.Add(e.Guid);
                    continue;
                }
                
                DLog.Alert($"Update runtimeId => {e.RuntimeId.name}", EDlogColor.YELLOW);

                // duplicate → regenerate
                var field = typeof(RuntimeId)
                    .GetField("guid",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

                if (field == null)
                    continue;

                var newGuid = System.Guid.NewGuid().ToString("N");
                field.SetValue(e.RuntimeId, newGuid);

                EditorUtility.SetDirty(e.RuntimeId);
            }

            AssetDatabase.SaveAssets();
            Refresh();
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace Galactic1.Code.Systems.Progression.Editor
{
    [CustomEditor(typeof(ProgressionUnlockDefinition))]
    public sealed class ProgressionUnlockDefinitionEditor
        : UnityEditor.Editor
    {
        private SerializedProperty _entries;

        private void OnEnable()
        {
            _entries = serializedObject.FindProperty("entries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawEntries();

            EditorGUILayout.Space(8);

            if (GUILayout.Button(
                    "Add Unlock Entry",
                    GUILayout.Height(28)))
            {
                AddUnlockEntry();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEntries()
        {
            EditorGUILayout.LabelField(
                "Unlock Entries",
                EditorStyles.boldLabel);

            EditorGUILayout.Space(4);

            if (_entries.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "No unlock entries configured.",
                    MessageType.Info);

                return;
            }

            for (int i = 0; i < _entries.arraySize; i++)
            {
                DrawEntry(i);

                if (i < _entries.arraySize - 1)
                    EditorGUILayout.Space(6);
            }
        }

        private void DrawEntry(int index)
        {
            SerializedProperty entry =
                _entries.GetArrayElementAtIndex(index);

            SerializedProperty id =
                entry.FindPropertyRelative("Id");

            SerializedProperty requirements =
                entry.FindPropertyRelative("Requirements");

            EditorGUILayout.BeginVertical("box");

            DrawEntryHeader(index);

            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(
                id,
                new GUIContent("Unlock Id"));

            EditorGUILayout.Space(8);

            DrawRequirements(requirements);

            EditorGUILayout.EndVertical();
        }

        private void DrawEntryHeader(int index)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                $"Unlock Entry {index + 1}",
                EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(
                    "Remove",
                    GUILayout.Width(70)))
            {
                RemoveUnlockEntry(index);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawRequirements(
            SerializedProperty requirements)
        {
            EditorGUILayout.LabelField(
                "Requirements",
                EditorStyles.boldLabel);

            EditorGUILayout.Space(3);

            if (requirements.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "No requirements configured. " +
                    "This unlock will be considered satisfied immediately.",
                    MessageType.Warning);
            }

            for (int i = 0; i < requirements.arraySize; i++)
            {
                SerializedProperty requirement =
                    requirements.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();

                UnlockRequirementDrawer.Draw(requirement);

                EditorGUILayout.EndVertical();

                if (GUILayout.Button(
                        "X",
                        GUILayout.Width(24),
                        GUILayout.Height(24)))
                {
                    RemoveRequirement(requirements, i);

                    EditorGUILayout.EndHorizontal();

                    break;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(3);
            }

            if (GUILayout.Button("+ Add Requirement"))
            {
                ShowAddRequirementMenu(requirements);
            }
        }

        private void ShowAddRequirementMenu(
            SerializedProperty requirements)
        {
            var menu = new GenericMenu();

            var types =
                UnlockRequirementEditorUtility.RequirementTypes;

            if (types.Count == 0)
            {
                menu.AddDisabledItem(
                    new GUIContent(
                        "No UnlockRequirement types found"));
            }
            else
            {
                foreach (var type in types)
                {
                    var capturedType = type;

                    menu.AddItem(
                        new GUIContent(
                            UnlockRequirementEditorUtility.GetDisplayName(
                                capturedType)),
                        false,
                        () => AddRequirement(
                            requirements,
                            capturedType));
                }
            }

            menu.ShowAsContext();
        }

        private void AddRequirement(
            SerializedProperty requirements,
            System.Type requirementType)
        {
            serializedObject.Update();

            int index = requirements.arraySize;

            requirements.InsertArrayElementAtIndex(index);

            SerializedProperty element =
                requirements.GetArrayElementAtIndex(index);

            element.managedReferenceValue =
                System.Activator.CreateInstance(requirementType);

            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveRequirement(
            SerializedProperty requirements,
            int index)
        {
            serializedObject.Update();

            requirements.DeleteArrayElementAtIndex(index);

            serializedObject.ApplyModifiedProperties();
        }

        private void AddUnlockEntry()
        {
            serializedObject.Update();

            int index = _entries.arraySize;

            _entries.InsertArrayElementAtIndex(index);

            SerializedProperty entry =
                _entries.GetArrayElementAtIndex(index);

            entry.FindPropertyRelative("Id")
                .objectReferenceValue = null;

            entry.FindPropertyRelative("Requirements")
                .ClearArray();

            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveUnlockEntry(int index)
        {
            serializedObject.Update();

            _entries.DeleteArrayElementAtIndex(index);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
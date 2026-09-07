
using System;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Code.Systems.Progression.Editor
{
    /// <summary>
    /// Draws a single polymorphic UnlockRequirement managed reference.
    ///
    /// Handles:
    /// - type selection;
    /// - managed-reference creation;
    /// - managed-reference clearing;
    /// - drawing concrete serialized fields.
    /// </summary>
    internal static class UnlockRequirementDrawer
    {
        public static void Draw(SerializedProperty property)
        {
            if (property == null)
                return;

            EditorGUILayout.BeginVertical("box");

            DrawHeader(property);

            if (property.managedReferenceValue != null)
            {
                EditorGUILayout.Space(4);
                DrawFields(property);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawHeader(SerializedProperty property)
        {
            var current = property.managedReferenceValue;

            string displayName = current == null
                ? "None"
                : UnlockRequirementEditorUtility.GetDisplayName(
                    current.GetType());

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                "Requirement",
                GUILayout.Width(EditorGUIUtility.labelWidth - 4));

            if (EditorGUILayout.DropdownButton(
                    new GUIContent(displayName),
                    FocusType.Passive))
            {
                ShowTypeMenu(property);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void ShowTypeMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();

            bool isNone = property.managedReferenceValue == null;

            menu.AddItem(
                new GUIContent("None"),
                isNone,
                () => SetManagedReference(property, null));

            menu.AddSeparator("");

            foreach (var type in UnlockRequirementEditorUtility.RequirementTypes)
            {
                Type capturedType = type;

                bool selected =
                    property.managedReferenceValue != null &&
                    property.managedReferenceValue.GetType() == capturedType;

                menu.AddItem(
                    new GUIContent(
                        UnlockRequirementEditorUtility.GetDisplayName(
                            capturedType)),
                    selected,
                    () => SetManagedReference(
                        property,
                        Activator.CreateInstance(capturedType)));
            }

            menu.ShowAsContext();
        }

        private static void SetManagedReference(
            SerializedProperty property,
            object value)
        {
            property.serializedObject.Update();

            property.managedReferenceValue = value;

            property.serializedObject.ApplyModifiedProperties();
        }

        private static void DrawFields(SerializedProperty property)
        {
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();

            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                if (SerializedProperty.EqualContents(
                        iterator,
                        endProperty))
                {
                    break;
                }

                EditorGUILayout.PropertyField(iterator, true);

                enterChildren = false;
            }
        }
    }
}
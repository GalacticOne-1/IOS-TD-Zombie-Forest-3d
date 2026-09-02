using System;
using System.Linq;
using Galactic1.Game.Meta.Items;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Tools
{
    public static class ConsumableBehaviourDrawer
    {
        private static Type[] _types;

        static ConsumableBehaviourDrawer()
        {
            _types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && typeof(ConsumableBehaviour).IsAssignableFrom(t))
                .ToArray();
        }

        public static void Draw(SerializedProperty property)
        {
            EditorGUILayout.BeginVertical("box");

            var current = property.managedReferenceValue;
            string currentName = current == null ? "None" : current.GetType().Name;

            EditorGUILayout.LabelField("Behaviour", EditorStyles.boldLabel);

            // 🔽 dropdown
            if (EditorGUILayout.DropdownButton(
                    new GUIContent(currentName),
                    FocusType.Passive))
            {
                var menu = new GenericMenu();

                foreach (var type in _types)
                {
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        property.serializedObject.Update();

                        property.managedReferenceValue = Activator.CreateInstance(type);

                        property.serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.ShowAsContext();
            }

            // 🔽 draw fields
            if (property.managedReferenceValue != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(property, true);
            }

            EditorGUILayout.EndVertical();
        }
    }
}
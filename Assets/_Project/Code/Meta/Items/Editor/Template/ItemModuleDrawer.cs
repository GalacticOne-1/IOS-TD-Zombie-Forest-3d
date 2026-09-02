using System;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    [CustomPropertyDrawer(typeof(ItemModule), true)]
    public class ItemModuleDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue == null)
                return EditorGUIUtility.singleLineHeight;

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue == null)
            {
                if (GUI.Button(position, "Select Module"))
                {
                    var menu = new GenericMenu();

                    var types = TypeCache.GetTypesDerivedFrom<ItemModule>();

                    foreach (var type in types)
                    {
                        if (type.IsAbstract) continue;

                        var captured = type;

                        menu.AddItem(new GUIContent(type.Name), false, () =>
                        {
                            property.managedReferenceValue =
                                Activator.CreateInstance(captured);

                            property.serializedObject.ApplyModifiedProperties();
                            property.serializedObject.Update();
                        });
                    }

                    menu.ShowAsContext();
                }

                return;
            }

            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}
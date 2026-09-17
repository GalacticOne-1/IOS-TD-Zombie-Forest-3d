
using UnityEditor;
using UnityEngine;
using Galactic1.Code.Systems.Tutorial.Authoring;

[CustomPropertyDrawer(typeof(TutorialTargetQuery), useForChildren: true)]
public sealed class TutorialTargetQueryDrawer : PropertyDrawer
{
    private static readonly System.Type[] Types =
    {
        null, // None
        typeof(TutorialFixedTargetQuery),
        typeof(TutorialInventoryItemQuery),
        typeof(TutorialInboxItemQuery),
        typeof(TutorialUnitSearchQuery),
        typeof(TutorialFacilityCardQuery),
        typeof(TutorialConstructionTabQuery),
    };

    private static readonly string[] Labels =
        { "None", "Fixed Target", "Inventory Item", "Inbox Item", "Unit Search", "Facility Card", "Construction Tab" };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var currentType = property.managedReferenceValue?.GetType();
        int currentIndex = System.Array.IndexOf(Types, currentType);
        if (currentIndex < 0) currentIndex = 0;

        var popupRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        int newIndex = EditorGUI.Popup(popupRect, label.text, currentIndex, Labels);

        if (newIndex != currentIndex)
        {
            property.managedReferenceValue =
                Types[newIndex] == null ? null : System.Activator.CreateInstance(Types[newIndex]);
        }

        if (property.managedReferenceValue != null)
        {
            var fieldsRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight,
                position.width, EditorGUI.GetPropertyHeight(property, true) - EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(fieldsRect, property, GUIContent.none, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUIUtility.singleLineHeight +
           (property.managedReferenceValue != null ? EditorGUI.GetPropertyHeight(property, true) : 0);
}
using UnityEditor;
using UnityEngine;

public static class EditorLabelStyles
{
    public static readonly GUIStyle Red;
    public static readonly GUIStyle Green;
    public static readonly GUIStyle Yellow;

    private static int h2 = 14;

    static EditorLabelStyles()
    {
        Red = new GUIStyle(EditorStyles.label);
        Red.normal.textColor = Color.red;
        Red.fontSize = h2;

        Green = new GUIStyle(EditorStyles.label);
        Green.normal.textColor = Color.green;
        Green.fontSize = h2;
        
        Yellow = new GUIStyle(EditorStyles.label);
        Yellow.normal.textColor = Color.yellow;
        Yellow.fontSize = h2;
    }
}
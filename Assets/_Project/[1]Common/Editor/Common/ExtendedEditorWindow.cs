using System;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Tools
{
    public class ExtendedEditorWindow : EditorWindow
    {
        protected SerializedObject serialObj;
        protected SerializedProperty currProperty, selectedProperty;
        private string selectedPropertyPath;
        
        public Color original = new Color(1,1,1);


        public Texture2D greenFlagTexture;
        public Texture2D redFlagTexture;





        #region GENERAL

        private void Awake()
        {
            Flags();
        }
        
        


        /// <summary>
        /// Для отслеживания изменений в редакторе
        /// </summary>
        /// <returns></returns>
        public bool HaveChanges() => EditorGUI.EndChangeCheck();
        
        /// <summary>
        /// for saving
        /// </summary>
        public void EditorEndCheck()
        {
            if (EditorGUI.EndChangeCheck())
            {
                serialObj.ApplyModifiedProperties();
            }
        }
        
        public void SaveAsset(ScriptableObject scriptableObject)
        {
            EditorUtility.SetDirty(scriptableObject);
            AssetDatabase.SaveAssets();
        }

        public void Flags()
        {
            // Create green icon
            greenFlagTexture = new Texture2D(10, 10);
            Color32[] pixels = new Color32[10 * 10];
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = Color.green;
            }
            greenFlagTexture.SetPixels32(pixels);
            greenFlagTexture.Apply();
            
            // Create red icon
            redFlagTexture = new Texture2D(10, 10);
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = Color.red;
            }
            redFlagTexture.SetPixels32(pixels);
            redFlagTexture.Apply();
        }
        

        #endregion
        
        
        
        


        
        
        
        
        
        
        public void DrawSidebar(SerializedProperty prop)
        {
            foreach (SerializedProperty p in prop)
            {
                if (GUILayout.Button(p.displayName))
                {
                    selectedPropertyPath = p.propertyPath;
                }
            }

            if (!string.IsNullOrEmpty(selectedPropertyPath))
            {
                selectedProperty = serialObj.FindProperty(selectedPropertyPath);
            }
        }
        public void DrawProperties(SerializedProperty prop, bool drawChildren)
        {
            string lastPropPath = String.Empty;
            foreach (SerializedProperty p in prop)
            {
                if (p.isArray && p.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUILayout.BeginHorizontal();
                    p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
                    EditorGUILayout.EndHorizontal();

                    if (p.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        DrawProperties(p,drawChildren);
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(lastPropPath) && p.propertyPath.Contains(lastPropPath))
                    {
                        continue;
                    }

                    lastPropPath = p.propertyPath;
                    EditorGUILayout.PropertyField(p, drawChildren);
                }
            }
        }



        #region LABEL
        
        
        /// <summary>
        /// Regular GUIStyle 
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="fontStyle"></param>
        /// <param name="textAnchor"></param>
        public GUIStyle GUIStyle_Label(int fontSize, FontStyle fontStyle = FontStyle.Normal, TextAnchor textAnchor = TextAnchor.LowerLeft)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = fontSize;
            style.fontStyle = fontStyle;
            style.alignment = textAnchor;
            return style;
        }

        /// <summary>
        /// Label
        /// </summary>
        /// <param name="data"></param>
        public void Label(CLabelData data)
        {
            GUI.contentColor = data.enabled ? data.colorON : data.colorOFF;
            EditorGUILayout.LabelField(data.name, data.style);
            GUI.contentColor = original;
        }
        
        
        /// <summary>
        /// Regular GUIStyle 
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="fontStyle"></param>
        /// <param name="textAnchor"></param>
        /// <returns></returns>
        public GUIStyle GUIStyle_Button(int fontSize = 12, FontStyle fontStyle = FontStyle.Normal, TextAnchor textAnchor = TextAnchor.LowerLeft)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fontSize = fontSize;
            style.fontStyle = fontStyle;
            style.alignment = textAnchor;
            return style;
        }
        
        

        #endregion
        
        
        #region Button

        /// <summary>
        /// Обычная кнопка
        /// </summary>
        /// <param name="name"></param>
        /// <param name="func"></param>
        public void Button(string name, float width, DFunc func)
        {
            if (GUILayout.Button(name, GUILayout.MaxWidth(width)))
            {
                func?.Invoke();
            }
        }
        
        /// <summary>
        /// Кнопка с изменением цвета
        /// </summary>
        /// <param name="name"></param>
        /// <param name="func"></param>
        public void Button(CButtonData data)
        {
            GUI.backgroundColor = data.enabled ? data.colorON : data.colorOFF;
            if (GUILayout.Button(data.name, GUILayout.MaxWidth(data.width), GUILayout.MaxHeight(data.height)))
            {
                data.func?.Invoke();
            }

            GUI.backgroundColor = original;
        }


        #endregion
        
        
        
        public GUIStyle FlagStatusStyle()
        {
            var style = new GUIStyle();
            style.fontSize = 14;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
            style.contentOffset = new Vector2(10, 0);
            return style;
        }

        
        
    }
}
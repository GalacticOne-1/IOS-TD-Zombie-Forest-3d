
using UnityEditor;
using UnityEngine;

namespace Galactic1.Tools
{
    
    public abstract class EditorABS : Editor
    {

        protected SerializedObject scriptSO;
        
        protected Color origin = new Color(1,1,1);
        
        
        
        protected void EditorEndCheck()
        {
            // for saving
            if (EditorGUI.EndChangeCheck())
            {
                Save();
                //GUI.FocusControl(null);
            }
        }
        
        protected void Save()
        {
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll();
        }

        
        
        

        #region Button

        /// <summary>
        /// Обычная кнопка
        /// </summary>
        /// <param name="name"></param>
        /// <param name="func"></param>
        protected void Button(string name, float width, DFunc func)
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
        protected void Button(CButtonData data)
        {
            GUI.backgroundColor = data.enabled ? data.colorON : data.colorOFF;
            if (GUILayout.Button(data.name, GUILayout.MaxWidth(data.width), GUILayout.MaxHeight(data.height)))
            {
                data.func?.Invoke();
            }

            GUI.backgroundColor = origin;
        }

        #endregion
        
        
        
    }
    
    public class CButtonData
    {
        public string name;
        public float width = 100, height = 30;
        public DFunc func;
        public bool enabled;
        public Color colorON = Color.green;
        public Color colorOFF = new Color(1,1,1);
    }
    
    public class CLabelData
    {
        public string name;
        public bool enabled;
        public GUIStyle style;
        public Color colorON = Color.green;
        public Color colorOFF = Color.red;
    }
}
using UnityEditor;
using UnityEngine;

namespace Galactic1.Tools
{
    public class ClickLoggerWindow : EditorWindow
    {
        private bool isLogging = false;

        [MenuItem("Tools/Click Logger")]
        public static void ShowWindow()
        {
            GetWindow<ClickLoggerWindow>("Click Logger");
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            GUILayout.Label("Click Logger Tool", EditorStyles.boldLabel);

            isLogging = GUILayout.Toggle(isLogging, "Enable Logging");

            EditorGUILayout.HelpBox("Click anywhere in the Scene View to log world position.", MessageType.Info);
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!isLogging) return;

            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                // Клик мыши левой кнопкой в сцене (без ALT)
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Debug.Log($"[ClickLogger] Hit Position: {hit.point}");
                }
                else
                {
                    // Если не попал в Collider, то всё равно покажем примерно где
                    Vector3 fallback = ray.origin + ray.direction * 10f;
                    Debug.Log($"[ClickLogger] Approximate Position (no hit): {fallback}");
                }

                e.Use(); // предотвращает другие реакции на клик
            }
        }
    }

}
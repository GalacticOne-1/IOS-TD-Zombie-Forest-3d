using UnityEngine;

namespace Galactic1
{

    public class FPSDisplay : MonoBehaviour
    {
        float deltaTime = 0.0f;

        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }

        void OnGUI()
        {
            int w = Screen.width, h = Screen.height;
            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(100, h-50, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 70;
            style.normal.textColor = Color.green;

            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.} FPS", fps);
            GUI.Label(rect, text, style);
        }
    }
}
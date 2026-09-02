
using UnityEditor;
using UnityEngine;

namespace Galactic1.EditorTools.PrefabScreenshot
{
    [System.Serializable]
    public class ScreenshotSettings
    {
        public DefaultAsset prefabFolder;
        public DefaultAsset outputFolder;
        
        

        public int imageSize = 1024;

        public float cameraPitch = -25f;
        public float cameraYaw = 35f;

        public float padding = 1f;

        public bool transparentBackground = true;

        public bool cropTransparentPixels = true;

        public bool useMSAA = true;

        public int antiAliasing = 8;

        public float lightIntensity = 1.3f;
        
        
        // Камера — полный контроль
        public float cameraDistanceMultiplier = 3f; // зум (множитель относительно радиуса bounds)
        public float fieldOfView = 70;
        public float cameraRoll = 0f;

        
        // Нормализованные смещения относительно размера объекта (диапазон, как правило, -1..1)
        public Vector3 targetOffsetNormalized = Vector3.zero;
        public Vector3 positionOffsetNormalized = Vector3.zero;
    }
}
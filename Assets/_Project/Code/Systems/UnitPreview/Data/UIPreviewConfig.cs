using UnityEngine;

namespace Galactic1.UI.CharacterPreview
{
    /// <summary>
    /// Конфигурация отображения 3D объекта в UI preview.
    /// Определяет положение камеры и модели.
    /// </summary>
    [CreateAssetMenu(
        fileName = "UIPreviewConfig",
        menuName = "Game Configs/Preview/UI Preview Config")]
    public class UIPreviewConfig : ScriptableObject
    {
        [Header("Camera")]
        public Vector3 cameraOffset = new(0f, 1.5f, -2f);
        public Vector3 lookOffset = new(0f, 1f, 0f);
        public float fieldOfView = 40f;

        [Header("Model")]
        public Vector3 modelRotation;
        public Vector3 modelOffset;
        public float scale = 1f;
    }
}
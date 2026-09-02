using UnityEngine;

namespace Galactic1.Code.Gameplay.Interaction
{
    /// <summary>
    /// Результат world raycast (AAA контракт)
    /// </summary>
    public struct WorldPointerHit
    {
        public Vector3 Position;
        public Vector3 Normal;

        public Collider Collider;
        public GameObject GameObject;
        
        /// <summary>
        /// Экранная позиция указателя в момент события.
        /// Не зависит от результата raycast — нужна для UI hit-тестов (Cancel Zone и т.п.).
        /// </summary>
        public Vector2 ScreenPosition;

        public bool IsValid;

        public static WorldPointerHit Invalid => new() { IsValid = false };
    }
}
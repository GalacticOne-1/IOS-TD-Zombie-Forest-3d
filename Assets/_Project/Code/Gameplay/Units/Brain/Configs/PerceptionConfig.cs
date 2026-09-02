using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    [CreateAssetMenu(fileName = "PerceptionConfig", menuName = "Game Configs/AI/Perception Config")]
    public sealed class PerceptionConfig : ScriptableObject
    {
        [Header("Sight")] 
        public float detectionRadius = 20f;
        public float updateInterval = 0.15f;

        [Header("Hearing")] [Tooltip("Радиус в котором зомби слышит звуки.")]
        public float hearingRadius = 30f;

        [Tooltip("Множитель чувствительности [0..1]. 1 = слышит на полный радиус.")] [Range(0f, 1f)]
        public float hearingSensitivity = 1f;
    }
}
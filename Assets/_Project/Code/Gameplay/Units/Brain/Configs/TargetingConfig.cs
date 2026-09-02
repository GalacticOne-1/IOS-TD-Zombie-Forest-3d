using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Game.Meta.Enemy
{
    /// <summary>
    /// Targeting / memory configuration layer.
    ///
    /// НЕ относится к perception (что видим).
    /// НЕ относится к combat (как атакуем).
    ///
    /// Отвечает за:
    ///   - удержание цели после потери LOS
    ///   - "память" о враге
    ///   - деградацию/забывание цели
    ///   - hysteresis поведения преследования
    /// </summary>
    [CreateAssetMenu(menuName = "Game Configs/AI/Targeting Config")]
    public sealed class TargetingConfig : ScriptableObject
    {
        [Header("Memory Retention")]
        public float LoseTargetRange = 18f;
        public float LoseTargetDelay = 4f;

        [Header("Memory Decay")]
        [Range(0f, 1f)]
        public float MemoryDecayRate = 0.15f;

        public float RetargetCooldown = 0.5f;

        [Header("Re-acquisition")]
        public float ReacquireRadius = 22f;
        public float RecentTargetBias = 0.25f;
    }
}
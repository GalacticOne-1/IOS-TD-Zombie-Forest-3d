using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Visual
{
    /// <summary>
    /// Global combat camera shake tuning config.
    ///
    /// USED BY:
    /// - CombatCameraShakeService
    /// - designers
    /// - balancing
    /// </summary>
    [CreateAssetMenu(menuName = "Game Configs/Combat/Combat Camera Shake Config")]
    public sealed class CombatCameraShakeConfig : ScriptableObject
    {
        [Header("Trauma")]
        public float maxTrauma = 1f;

        public float traumaDecay = 1.8f;

        [Header("Position")]
        public float maxPositionOffset =  0.35f;

        [Header("Rotation")]
        public float maxRotation = 2.5f;

        [Header("Suppression")]
        public float suppressionMax = 0.08f;

        [Header("Budget")]
        public float shakeBudget = 1f;

        [Header("Explosion")]
        public float explosionLowFrequency = 4f;

        public float explosionHighFrequency = 24f;

        [Header("Suppression")]
        public float suppressionDecay = 2.5f;
    }
}
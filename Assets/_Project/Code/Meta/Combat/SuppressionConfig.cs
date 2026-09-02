using UnityEngine;

namespace Galactic1.Code.Data.Combat
{
    /// <summary>
    /// Data-driven suppression tuning config.
    ///
    /// Suppression is psychological pressure applied to a unit
    /// when it receives fire — separate from HP damage.
    /// High suppression forces AI into defensive behaviour.
    ///
    /// Used by SuppressionSystem.
    /// </summary>
    [CreateAssetMenu(
        menuName = "Game Configs/Combat/Suppression Config",
        fileName = "SuppressionConfig")]
    public sealed class SuppressionConfig : ScriptableObject
    {
        [Tooltip("How much suppression is added per 1 point of final damage received.")] [Range(0f, 2f)]
        public float DamageToSuppression = 0.5f;

        [Tooltip("Maximum suppression value a unit can accumulate.")] [Range(0f, 200f)]
        public float MaxSuppression = 100f;

        [Tooltip("Suppression decay per second when not under fire.")] [Range(0f, 50f)]
        public float DecayPerSecond = 5f;
    }
}
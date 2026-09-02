using UnityEngine;

namespace Galactic1.Game.Meta.Enemy
{
    /// <summary>
    /// Group coordination and encirclement tuning.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ZombiePack",
        menuName = "Game Configs/Enemy/Zombie Pack")]
    public sealed class ZombiePackConfig : ScriptableObject
    {
        [Header("Encirclement")]
        public float EncircleRadius = 1.8f;

        public float SlotAngleStep = 45f;

        public float MinSlotDistance = 0.9f;

        [Header("Utility")]
        public float PackSlotWeight = 0.25f;

        public int MaxAttackersPerTarget = 6;
    }
}
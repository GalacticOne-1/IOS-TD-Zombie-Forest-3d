using System;
using UnityEngine;

namespace Galactic1.Game.Meta.Enemy
{
    /// <summary>
    /// Physical and survivability stats.
    /// Immutable runtime stat source.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnemyStats",
        menuName = "Game Configs/Enemy/Enemy Stats")]
    public sealed class EnemyStatsConfig : ScriptableObject
    {
        [Serializable]
        public struct StatsData
        {
            [Header("Core")]
            public float Health;
            public float Armor;


            [Header("Stagger")]
            public float Poise;
            public float StunResistance;
        }

        [field: SerializeField] public StatsData BaseStats { get; private set; }
    }
}
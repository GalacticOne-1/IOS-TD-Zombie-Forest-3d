using UnityEngine;

namespace Galactic1.Game.Meta.Enemy
{
    /// <summary>
    /// Combat execution and attack rules.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnemyCombat",
        menuName = "Game Configs/Enemy/Enemy Combat")]
    public sealed class EnemyCombatConfig : ScriptableObject
    {
        [Header("Damage")]
        public float Damage;

        public float CritChance;

        [Header("Attack")]
        public float AttackRange;

        public float AttackCooldown;

        public float Windup;

        public float Recovery;

        [Header("Combat Behaviour")]
        public bool CanStrafe;

        public bool CanChainAttacks;

        public bool CanUseSpecialAttack;
    }
}
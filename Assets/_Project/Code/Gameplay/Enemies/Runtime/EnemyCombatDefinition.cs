namespace Galactic1.Code.Systems.Raid.Enemies
{
    /// <summary>
    /// Immutable runtime combat payload.
    ///
    /// Содержит только runtime-safe combat данные.
    /// НЕ содержит ScriptableObject references.
    /// НЕ содержит scene/presentation data.
    ///
    /// Используется:
    ///   - Utility AI Actions
    ///   - Combat systems
    ///   - Attack states
    ///   - Damage pipeline
    ///   - Ability evaluators
    /// </summary>
    public sealed class EnemyCombatDefinition
    {
        // ─────────────────────────────────────────────────────────────
        // Damage
        // ─────────────────────────────────────────────────────────────

        public float Damage { get; }

        public float CritChance { get; }

        // ─────────────────────────────────────────────────────────────
        // Attack
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Дистанция входа в melee attack.
        /// </summary>
        public float AttackRange { get; }

        /// <summary>
        /// Время между атаками.
        /// </summary>
        public float AttackCooldown { get; }

        /// <summary>
        /// Windup перед нанесением урона.
        /// </summary>
        public float Windup { get; }

        /// <summary>
        /// Recovery после атаки.
        /// </summary>
        public float Recovery { get; }

        // ─────────────────────────────────────────────────────────────
        // Behaviour Flags
        // ─────────────────────────────────────────────────────────────

        public bool CanStrafe { get; }

        public bool CanChainAttacks { get; }

        public bool CanUseSpecialAttack { get; }

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        public EnemyCombatDefinition(
            float damage,
            float critChance,
            float attackRange,
            float attackCooldown,
            float windup,
            float recovery,
            bool canStrafe,
            bool canChainAttacks,
            bool canUseSpecialAttack)
        {
            Damage = damage;
            CritChance = critChance;


            AttackRange = attackRange;
            AttackCooldown = attackCooldown;

            Windup = windup;
            Recovery = recovery;

            CanStrafe = canStrafe;
            CanChainAttacks = canChainAttacks;
            CanUseSpecialAttack = canUseSpecialAttack;
        }
    }
}
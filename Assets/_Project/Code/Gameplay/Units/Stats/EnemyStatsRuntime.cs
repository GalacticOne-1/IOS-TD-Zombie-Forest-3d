using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units.Stats;

namespace Galactic1.Code.Systems.Runtime.Enemy
{
    /// <summary>
    /// Runtime stat-controller врага.
    ///
    /// Использует общий stat-pipeline:
    /// BaseStats
    ///     ↓
    /// Buffs / equipment / modifiers
    ///     ↓
    /// CalculatedStats
    ///     ↓
    /// CurrentStats
    ///
    /// EnemyStatsRuntime добавляет только enemy-specific helpers.
    /// </summary>
    public sealed class EnemyStatsRuntime : StatsRuntimeBase
    {
        // ─────────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────────

        public float NormalizedHealth =>
            GetMax(StatId.Health) <= 0f
                ? 0f
                : GetCurrent(StatId.Health) / GetMax(StatId.Health);

        public float MoveSpeed =>
            GetCurrent(StatId.MoveSpeed);

        public float AttackDamage =>
            GetCurrent(StatId.Damage);

        public float Armor =>
            GetCurrent(StatId.Armor);

        // ─────────────────────────────────────────────────────────────
        //  Constructor
        // ─────────────────────────────────────────────────────────────

        public EnemyStatsRuntime(
            string owner,
            Dictionary<StatId, float> baseStats,
            IEquipmentStatsProvider equipmentStatsProvider)
            : base(owner, baseStats, equipmentStatsProvider)
        {
            ActivateLive();
        }
        
        protected override void ActivateLive()
        {
            // ограничиваем под max
            ClampAllCurrentStats();
        }

        // ─────────────────────────────────────────────────────────────
        //  Combat API
        // ─────────────────────────────────────────────────────────────

        public void ApplyDamage(float rawDamage)
        {
            if (IsDead)
                return;

            float armor = GetCurrent(StatId.Armor);

            float finalDamage = rawDamage - armor;

            if (finalDamage < 1f)
                finalDamage = 1f;

            ModifyStat(StatId.Health, -finalDamage);
        }

        public void ApplyHeal(float amount)
        {
            if (IsDead)
                return;

            ModifyStat(StatId.Health, amount);
        }
    }
}
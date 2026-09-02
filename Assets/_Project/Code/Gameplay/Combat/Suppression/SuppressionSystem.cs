using Galactic1.Code.Data.Combat;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Suppression
{
    /// <summary>
    /// Applies tactical suppression pressure to a unit.
    ///
    /// Suppression is stored in StatId.Suppression (added in Phase 2).
    /// It is separate from HP — no damage is dealt here.
    ///
    /// Used by:
    /// - BurstSuppressionAggregator (main consumer)
    /// - AI reaction systems
    /// - Morale systems
    /// </summary>
    public sealed class SuppressionSystem
    {
        private readonly SuppressionConfig _config;

        public SuppressionSystem(SuppressionConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Adds suppression to a unit proportional to the damage amount.
        /// Clamps to MaxSuppression defined in config.
        /// </summary>
        public void Apply(IUnitSceneContext unit, float damage)
        {
            if (unit == null || unit.Stats.IsDead)
                return;

            float amount = damage * _config.DamageToSuppression;

            unit.Stats.ModifyStat(StatId.Suppression, amount);

            // Clamp to max — ModifyStat already clamps to CalculatedStats max,
            // but suppression max may not be set in base stats.
            // Enforce config ceiling explicitly.
            float current = unit.Stats.Get(StatId.Suppression).Value;
            if (current > _config.MaxSuppression)
                unit.Stats.ModifyStat(StatId.Suppression, _config.MaxSuppression - current);
        }

        /// <summary>
        /// Decays suppression over time. Call from unit tick.
        /// </summary>
        public void Decay(IUnitSceneContext unit, float dt)
        {
            if (unit == null || unit.Stats.IsDead)
                return;

            float current = unit.Stats.Get(StatId.Suppression).Value;
            if (current <= 0f)
                return;

            float decay = _config.DecayPerSecond * dt;
            unit.Stats.ModifyStat(StatId.Suppression, -decay);
        }
    }
}
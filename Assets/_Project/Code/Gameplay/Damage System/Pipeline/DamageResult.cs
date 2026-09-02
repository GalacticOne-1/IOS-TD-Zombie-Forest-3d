// =====================================================================
// ПРОВЕРИТЬ В ПРОЕКТЕ: DamageResult.cs
//
// WeaponFireService Phase 2 использует:
//   damage.Applied      — был ли урон применён (не отменён DeadCheckStep)
//   damage.FinalDamage  — итоговый урон после всех шагов
//
// Текущий DamagePipeline.Execute() возвращает:
//   return new DamageResult(context.BaseDamage, context.Damage, context.IsCancelled);
//
// Значит DamageResult должен выглядеть так:
// =====================================================================

namespace Galactic1.Code.Gameplay.Damage
{
    /// <summary>
    /// Final damage execution result.
    /// Produced by DamagePipeline.Execute().
    /// Consumed by:
    /// - WeaponFireService (suppression, events)
    /// - AoEService
    /// </summary>
    public readonly struct DamageResult
    {
        /// <summary>True if damage was actually applied (pipeline was not cancelled).</summary>
        public readonly bool Applied;

        /// <summary>Final damage value after all pipeline steps.</summary>
        public readonly float FinalDamage;

        /// <summary>True if the target died from this hit.</summary>
        public readonly bool Killed;

        public DamageResult(float baseDamage, float finalDamage, bool cancelled)
        {
            Applied = !cancelled;
            FinalDamage = finalDamage;
            // Killed is not tracked in current pipeline —
            // read from target.Stats.IsDead after Apply if needed.
            Killed = false;
        }
    }
}
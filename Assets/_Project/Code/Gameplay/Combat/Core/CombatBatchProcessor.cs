using Galactic1.Code.Gameplay.Combat.Burst;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Gameplay.Combat.Hit;
using Galactic1.Code.Gameplay.Damage;
using Galactic1.Code.Systems.Raid;

namespace Galactic1.Code.Gameplay.Combat
{
    /// <summary>
    /// Обрабатывает HitBatchResult: применяет урон и передаёт результат в CombatEventService.
    ///
    /// ИЗМЕНЕНИЯ после рефакторинга:
    ///   Было: CombatBatchProcessor сам поднимал CombatHitEvent / CombatDeathEvent
    ///         и напрямую работал с BurstSuppressionAggregator.
    ///   Стало: всё делегировано в CombatEventService.
    ///
    /// CombatBatchProcessor теперь отвечает ТОЛЬКО за:
    ///   HitBatchResult → foreach HitResult → DamageService.ApplyDamage() → CombatEventService.RaiseHit()
    ///
    /// Супрессия сбрасывается один раз через CombatEventService.FlushSuppression()
    /// после обхода всех попаданий — семантика не изменилась.
    /// </summary>
    public sealed class CombatBatchProcessor
    {
        private readonly CombatEventService _combatEvents;

        public CombatBatchProcessor(CombatEventService combatEvents)
        {
            _combatEvents = combatEvents;
        }

        /// <summary>
        /// Обрабатывает все HitResult в батче.
        ///
        /// miss         → пропуск (CombatMissEvent поднимает WeaponFireService)
        /// environment  → пропуск (нет цели — нет gameplay-события)
        /// unit hit     → DamageService.ApplyDamage + CombatEventService.RaiseHit
        ///
        /// После цикла — один FlushSuppression на батч.
        /// </summary>
        public void Process(HitBatchResult batch, IUnitSceneContext attacker)
        {
            foreach (HitResult result in batch.Hits)
            {
                if (!result.Hit)
                    continue; // CombatMissEvent — см. WeaponFireService

                if (result.Target == null)
                    continue; // Environment hit — визуальный путь отдельно

                DamageResult damage = DamageService.ApplyDamage(
                    attacker,
                    result.Target,
                    result.Damage,
                    DamageType.Bullet,
                    new HitInfo
                    {
                        Point = result.Point,
                        Normal = result.Normal,
                        BodyPart = result.BodyPart,
                        Surface = result.Surface
                    });

                if (!damage.Applied)
                    continue;

                _combatEvents.RaiseHit(
                    attacker,
                    result.Target,
                    damage,
                    new HitInfo
                    {
                        Point = result.Point,
                        Normal = result.Normal,
                        BodyPart = result.BodyPart,
                        Surface = result.Surface
                    },
                    result.ShotDirection);
            }

            // Один flush на батч — супрессия пишется per-unit, per-burst.
            _combatEvents.FlushSuppression();
        }
    }
}
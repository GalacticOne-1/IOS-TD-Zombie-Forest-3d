using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Gameplay.Combat.Suppression;
using Galactic1.Code.Gameplay.Damage;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat
{
    /// <summary>
    /// Единственный источник боевых событий в системе.
    ///
    /// ОТВЕТСТВЕННОСТЬ:
    ///   — Принимает результат уже применённого урона (DamageResult).
    ///   — Публикует CombatHitEvent / CombatDeathEvent через EventBus.
    ///   — Аккумулирует супрессию через BurstSuppressionAggregator.
    ///
    /// НЕ ДЕЛАЕТ:
    ///   — Не считает урон.
    ///   — Не модифицирует статы.
    ///   — Не применяет эффекты.
    ///   — Не знает об источнике урона (пуля, взрыв, яд, огонь).
    ///
    /// МАСШТАБИРОВАНИЕ:
    ///   Новый источник урона (melee, poison, fire, DoT) вызывает только
    ///   DamageService.ApplyDamage() + CombatEventService.RaiseHit().
    ///   Никаких изменений в EventBus, UI, анимационных системах.
    ///
    /// ЖИЗНЕННЫЙ ЦИКЛ:
    ///   Один экземпляр на рейд. Создаётся в RaidContext вместе с остальными
    ///   боевыми сервисами. Stateless кроме BurstSuppressionAggregator.
    /// </summary>
    public sealed class CombatEventService : IGameService
    {
        private readonly SuppressionSystem _suppression;
        private readonly BurstSuppressionAggregator _suppressionAggregator;

        public CombatEventService(
            SuppressionSystem suppression,
            BurstSuppressionAggregator suppressionAggregator)
        {
            _suppression = suppression;
            _suppressionAggregator = suppressionAggregator;
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// Вызывается после успешного применения урона.
        ///
        /// Публикует:
        ///   CombatHitEvent  — всегда при damage.Applied == true
        ///   CombatDeathEvent — если цель погибла
        ///
        /// Параметры:
        ///   attacker      — атакующий (может быть null для DoT без владельца)
        ///   target        — получивший урон юнит
        ///   damage        — результат из DamageService.ApplyDamage()
        ///   hitInfo       — точка, нормаль, часть тела, поверхность
        ///   shotDirection — направление выстрела (Vector3.zero для взрывов/DoT)
        /// </summary>
        public void RaiseHit(
            IUnitSceneContext attacker,
            IUnitSceneContext target,
            DamageResult result,
            HitInfo hitInfo,
            Vector3 shotDirection = default)
        {
            if (!result.Applied) return;
            if (target == null) return;

            // Супрессия аккумулируется здесь — не в каждом источнике урона.
            _suppressionAggregator.Add(target, result.FinalDamage);

            EventBus<CombatHitEvent>.Raise(new CombatHitEvent(
                attacker,
                target,
                result.FinalDamage,
                hitInfo.Point,
                hitInfo.Normal,
                shotDirection,
                hitInfo.Surface,
                hitInfo.BodyPart));

            if (target.Stats.IsDead)
            {
                EventBus<CombatDeathEvent>.Raise(new CombatDeathEvent(
                    target,
                    attacker,
                    hitInfo.Point));
            }
        }
        
        /// <summary>
        /// Вызывается после успешного применения урона.
        /// <br/>Просто для отображения полоски хп
        /// </summary>
        public void RaiseHit(
            IUnitSceneContext target,
            DamageResult result,
            HitInfo hitInfo,
            Vector3 shotDirection = default)
        {
            if (!result.Applied) return;
            if (target == null) return;

            EventBus<CombatHitEvent>.Raise(new CombatHitEvent(
                null,
                target,
                result.FinalDamage,
                hitInfo.Point,
                hitInfo.Normal,
                shotDirection,
                hitInfo.Surface,
                hitInfo.BodyPart));
        }

        /// <summary>
        /// Сбрасывает накопленную супрессию одним вызовом в конце батча.
        /// Вызывается CombatBatchProcessor.Process() после обхода всех hits.
        /// Для AoE / melee — вызывать после обработки всех целей в радиусе.
        /// </summary>
        public void FlushSuppression()
        {
            _suppressionAggregator.Flush(_suppression);
        }

        // ── Расширения (будущие события, один метод = один тип события) ──

        // public void RaiseCriticalHit(...) =>
        //     EventBus<CriticalHitEvent>.Raise(...);
        //
        // public void RaiseArmorBlocked(...) =>
        //     EventBus<ArmorBlockedEvent>.Raise(...);
        //
        // public void RaiseRicochet(...) =>
        //     EventBus<RicochetEvent>.Raise(...);
        //
        // public void RaiseDamagePopup(...) =>
        //     EventBus<DamagePopupEvent>.Raise(...);
    }
}
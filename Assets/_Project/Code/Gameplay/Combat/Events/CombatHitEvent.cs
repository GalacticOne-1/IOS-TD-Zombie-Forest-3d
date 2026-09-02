using Galactic1.Code.Gameplay.Combat.Data;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    /// <summary>
    /// Бросается когда попадание подтверждено и урон применён.
    ///
    /// Авторитетное gameplay-событие.
    ///
    /// Используется для:
    /// — Активации полоски HP (Show + Reset timer)
    /// — AI-реакций на урон
    /// — Подавления (Suppression)
    /// — Quest / objective систем
    /// — Аналитики
    /// — CombatEventRouter → VisualImpactEvent / AudioImpactEvent
    ///
    /// НЕ используется для синхронизации HP в UI.
    /// Для обновления значения HP используется HealthChangedEvent.
    ///
    /// Заполняется в CombatBatchProcessor после применения урона.
    /// </summary>
    public readonly struct CombatHitEvent : IEvent
    {
        public readonly IUnitSceneContext Attacker;
        public readonly IUnitSceneContext Target;

        /// <summary>Финальный урон после полного пайплайна.</summary>
        public readonly float Damage;

        public readonly Vector3 Point;

        /// <summary>Нормаль из HitResult.Normal (от raycast).</summary>
        public readonly Vector3 Normal;

        public readonly Vector3 ShotDirection;

        public readonly SurfaceType Surface;
        public readonly BodyPartType BodyPart;

        public CombatHitEvent(
            IUnitSceneContext attacker,
            IUnitSceneContext target,
            float damage,
            Vector3 point,
            Vector3 normal,
            Vector3 shotDirection,
            SurfaceType surface,
            BodyPartType bodyPart)
        {
            Attacker = attacker;
            Target = target;
            Damage = damage;
            Point = point;
            Normal = normal;
            ShotDirection = shotDirection;
            Surface = surface;
            BodyPart = bodyPart;
        }
    }
}
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    /// <summary>
    /// Бросается когда юнит умирает от боевого урона.
    ///
    /// Авторитетное gameplay-событие.
    ///
    /// Используется:
    /// — EnemyHealthBarSystem (немедленный возврат виджета в пул)
    /// — Objective системы
    /// — Лут-спавн
    /// — AI squad awareness
    /// — UIFeedbackSystem (kill indicator)
    /// — Аналитика
    /// </summary>
    public readonly struct CombatDeathEvent : IEvent
    {
        public readonly IUnitSceneContext Victim;
        public readonly IUnitSceneContext Killer;
        public readonly Vector3 Point;

        public CombatDeathEvent(
            IUnitSceneContext victim,
            IUnitSceneContext killer,
            Vector3 point)
        {
            Victim = victim;
            Killer = killer;
            Point = point;
        }
    }
}
using System;
using Galactic1.Code.Gameplay.Units.Brain.Zombie;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Blackboard
{
    public enum SiegeObjective { Headquarters, Wall, Player }

    /// <summary>
    /// Siege-специфичная память AI. Наследует всю Raid-память (pack, aggro,
    /// noise, combat, alert, hysteresis из EnemyBlackboard) и добавляет только
    /// то, чего там нет.
    ///
    /// ТРЕБУЕТ: EnemyBlackboard больше не sealed (см. Modified/EnemyBlackboard.cs).
    /// </summary>
    public class SiegeBlackboard : EnemyBlackboard
    {
        /// <summary>Текущая стратегическая цель. HQ — дефолт, никогда не забывается насовсем.</summary>
        public SiegeObjective CurrentObjective = SiegeObjective.Headquarters;

        /// <summary>Резолвится один раз при первом Fill() и кешируется — HQ не двигается.</summary>
        public ITargetInfo Headquarters;

        /// <summary>Текущая подтверждённая блокирующая путь стена (null если путь свободен
        /// или блокировка не доказана — см. SiegePathService, инвариант
        /// PathBlocked==true ⇒ CurrentWall!=null соблюдается конструктивно).</summary>
        public ITargetInfo CurrentWall;

        public float LastObjectiveSwitchTime;

        public bool PathBlocked;

        /// <summary>Точка блокировки — последний валидный corner конкретного path-результата,
        /// полученный через UnitMover.OnPathComputed (per-request, не устаревший).</summary>
        public Vector3 LastKnownBlockedPosition;
        
        /// <summary>NEW — sticky-выбор attack point (ТЗ п.15): пока точка валидна,
        /// переиспользуется между think-тиками, чтобы зомби не метался между
        /// AttackPoint_00..09.</summary>
        public Transform CurrentAttackPoint;

        public bool ReacquireAttackPoint;
        
        /// <summary>NEW — TargetId цели, под которую выдана текущая активная команда.
        /// Используется SiegeDecisionController.EnsureClean(), чтобы отличить
        /// "команда для той же цели переиздана" (ничего форсировать не нужно) от
        /// "цель реально сменилась" (нужно принудительно выйти из MeleeEngaging,
        /// иначе ChaseCommand будет молча отклонён FSM — см. ChaseCommand.CanExecute).</summary>
        public string ActiveCommandTargetId;

        /// <summary>Lifecycle-подписка на UnitMover.OnPathComputed. Снимается в Dispose().</summary>
        public Action PathComputedUnsubscribe;

        /// <summary>Lifecycle-подписка на IRaidFacilityRuntime.OnDestroyed текущей стены.
        /// Снимается при смене/потере стены и в Dispose().</summary>
        public Action WallDestroyedUnsubscribe;

        public SiegeBlackboard(PackReservationService packReservation) : base(packReservation) { }
    }
}

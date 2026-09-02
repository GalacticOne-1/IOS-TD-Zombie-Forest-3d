using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Приоритет цели: Player > Wall > HQ (абсолютный порядок из ТЗ).
    /// Player переключается немедленно, без cooldown. Wall/HQ защищены
    /// cooldown-ом против flickering между objective на границе состояний.
    /// </summary>
    public sealed class SiegeObjectiveResolver
    {
        private const float SwitchCooldown = 0.5f;

        // NEW — используется только чтобы отличить "игрок в прямой досягаемости
        // атаки" от "игрок просто виден". PathBlocked не содержит информации о
        // пути конкретно ДО игрока (только о HQ-маршруте), поэтому дешёвая и
        // безопасная эвристика — прямая дистанция без запроса нового path.
        private readonly float _attackRange;
        private readonly float _loseTargetRange;

        public SiegeObjectiveResolver(
            float attackRange,
            float loseTargetRange)
        {
            _attackRange = attackRange;
            _loseTargetRange = loseTargetRange;
        }

        public SiegeObjective Resolve(
            UnitInstance unit,
            SiegeAIContext ctx,
            SiegeBlackboard blackboard)
        {
            bool playerInChaseRange =
                ctx.HasVisibleTarget &&
                ctx.DistanceToVisibleTarget <= _loseTargetRange;

            bool playerInAttackRange =
                ctx.HasVisibleTarget &&
                ctx.DistanceToVisibleTarget <= _attackRange;

            bool playerDirectlyEngageable =
                playerInAttackRange ||
                (playerInChaseRange && !ctx.PathBlocked);

            SiegeObjective desired;

            if (playerDirectlyEngageable)
            {
                desired = SiegeObjective.Player;
            }
            else if (ctx.PathBlocked && ctx.BlockingWall != null)
            {
                desired = SiegeObjective.Wall;
            }
            else
            {
                desired = SiegeObjective.Headquarters;
            }

            if (desired == blackboard.CurrentObjective)
                return desired;

            if (desired == SiegeObjective.Player ||
                Time.time - blackboard.LastObjectiveSwitchTime >= SwitchCooldown)
            {
                var previous = blackboard.CurrentObjective;

                blackboard.CurrentObjective = desired;
                blackboard.LastObjectiveSwitchTime = Time.time;

                // NEW:
                // При новом входе в HQ objective заново выбираем ближайший AttackPoint.
                if (desired == SiegeObjective.Headquarters &&
                    previous != SiegeObjective.Headquarters)
                {
                    blackboard.ReacquireAttackPoint = true;
                }
                
                return desired;
            }

            return blackboard.CurrentObjective;
        }
    }
}

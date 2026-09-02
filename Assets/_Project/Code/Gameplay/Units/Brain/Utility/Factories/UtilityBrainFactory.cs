using System.Collections.Generic;
using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Brain.Utility.Core;
using Galactic1.Code.Gameplay.Units.Brain.Zombie;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Meta.Enemy;

namespace Galactic1.Code.Gameplay.Units.Brain.Core
{
    /// <summary>
    /// ИЗМЕНЕНИЯ: Create(def) → Create(profile, def). Старая реализация Create()
    /// стала BuildRaidBrain() почти без изменений — Raid-поведение идентично.
    /// </summary>
    public static class UtilityBrainFactory
    {
        public static IUnitBrain Create(EnemyAIProfile profile, EnemyRuntimeDefinition def)
        {
            return profile switch
            {
                EnemyAIProfile.Siege => BuildSiegeBrain(def),
                _ => BuildRaidBrain(def),
            };
        }

        // ── Raid — БЕЗ ИЗМЕНЕНИЙ ──────────────────────────────────────────
        private static UtilityUnitBrain BuildRaidBrain(EnemyRuntimeDefinition def)
        {
            var blackboard = BuildBlackboard(def);

            var actions = new List<IAIAction>
            {
                new AttackAction(def.CombatDefinition),
                new PackChaseAction(def),
                new SearchAction(def),
                new InvestigateAction(def),
                new RoamAction(),
            };

            return new UtilityUnitBrain(actions, def.BrainDefinition, def.TargetingDefinition, blackboard);
        }

        // ── Siege ────────────────────────────────────────────────────────────
        private static SiegeUtilityBrain BuildSiegeBrain(EnemyRuntimeDefinition def)
        {
            var gameLoopContext = ServiceLocator.Current.Get<GameSession>().GameLoopContext;
            var blackboard = BuildSiegeBlackboard(def);
            var facilityRepository = ServiceLocator.Current.Get<BaseFacilityRepository>();
            var pathService = new SiegePathService(gameLoopContext, facilityRepository);
            var objectiveResolver = new SiegeObjectiveResolver(
                def.CombatDefinition.AttackRange,
                def.TargetingDefinition.LoseTargetRange);

            // CHANGED: список IAIAction для Siege больше не строится —
            // единственный источник команд теперь SiegeDecisionController.
            var decisionController = new SiegeDecisionController(
                def.CombatDefinition.AttackRange,
                def.MovementDefinition.RunSpeed,
                def.BrainDefinition);

            return new SiegeUtilityBrain(
                def.BrainDefinition, def.TargetingDefinition,
                blackboard, pathService, objectiveResolver, decisionController);
        }

        private static EnemyBlackboard BuildBlackboard(EnemyRuntimeDefinition def)
        {
            var coordinator = ServiceLocator.Current.Get<PackCoordinator>();
            var reservation = new PackReservationService(coordinator, def.Pack);
            return new EnemyBlackboard(reservation);
        }

        private static SiegeBlackboard BuildSiegeBlackboard(EnemyRuntimeDefinition def)
        {
            var coordinator = ServiceLocator.Current.Get<PackCoordinator>();
            var reservation = new PackReservationService(coordinator, def.Pack);
            return new SiegeBlackboard(reservation);
        }
    }
}

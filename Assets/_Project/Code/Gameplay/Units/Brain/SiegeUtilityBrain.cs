using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Brain.Utility.Core;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Core
{
    /// <summary>
    /// Siege-версия брейна. В отличие от Raid UtilityUnitBrain, здесь нет
    /// utility-score арбитража между actions — решение Chase/Attack внутри
    /// текущего objective принимает SiegeDecisionController детерминированно.
    /// Единственная задача этого класса — построить context и один раз за
    /// Think() вызвать controller.Tick(), который и есть единственный
    /// источник StateMachine.Execute() в Siege.
    /// </summary>
    public sealed class SiegeUtilityBrain : IUnitBrain, IEnemyBrainWithBlackboard
    {
        private readonly EnemyAIDefinition _brainDef;
        private readonly TargetingDefinition _targeting;
        private readonly float _thinkInterval;
        private UnitInstance _unit;

        private readonly SiegeAIContext _ctx;
        private readonly SiegeBlackboard _blackboard;
        private readonly SiegePathService _pathService;
        private readonly SiegeObjectiveResolver _objectiveResolver;
        private readonly SiegeDecisionController _decisionController;

        private float _nextThinkTime;

        public bool IsEnabled { get; set; } = true;
        public EnemyBlackboard Blackboard => _blackboard;

        public SiegeUtilityBrain(
            EnemyAIDefinition brainDefinition,
            TargetingDefinition targetingDefinition,
            SiegeBlackboard blackboard,
            SiegePathService pathService,
            SiegeObjectiveResolver objectiveResolver,
            SiegeDecisionController decisionController)
        {
            _brainDef = brainDefinition;
            _targeting = targetingDefinition;
            _thinkInterval = brainDefinition.ThinkInterval > 0f ? brainDefinition.ThinkInterval : 0.2f;
            _blackboard = blackboard;
            _pathService = pathService;
            _objectiveResolver = objectiveResolver;
            _decisionController = decisionController;
            _ctx = new SiegeAIContext();
        }

        public void Initialize(UnitInstance unit) => _unit = unit;

        public void Tick(float dt)
        {
            if (!IsEnabled) return;
            _nextThinkTime -= dt;
            if (_nextThinkTime > 0f) return;
            _nextThinkTime += _thinkInterval;
            Think(dt);
        }

        private void Think(float dt)
        {
            // Единственное место, определяющее CurrentObjective — не изменено.
            SiegeAIContextBuilder.Fill(
                _unit, dt, _ctx, _blackboard, _targeting, _pathService, _objectiveResolver);

            var state = _ctx.CurrentState;
            if (state == UnitStateId.Suppressed
                || state == UnitStateId.Dying
                || state == UnitStateId.Dead)
                return;

#if UNITY_EDITOR
            Debug.Log($"[Siege] {_unit.name} objective={_blackboard.CurrentObjective} state={state}");
#endif
            // Единственный вызов за Think() — ровно один StateMachine.Execute()
            // (или ни одного, если ни одна ветка не применима).
            _decisionController.Tick(_unit, _ctx, _blackboard);
        }

        public void OnPlayerCommand(IUnitCommand command)
        {
        }

        public void OnStateChanged(UnitStateId newState)
        {
            if (newState == UnitStateId.Dying || newState == UnitStateId.Dead)
                _blackboard.ReleasePackSlot(_unit);
        }

        public void Dispose()
        {
            _blackboard.ReleasePackSlot(_unit);
            _pathService.Unsubscribe(_blackboard);
        }
    }
}
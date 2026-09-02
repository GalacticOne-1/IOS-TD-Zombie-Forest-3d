using System.Collections.Generic;
using System.Text;
using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Brain.Utility.Core;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Core
{
    public sealed class UtilityUnitBrain : IUnitBrain, IEnemyBrainWithBlackboard
    {
        private readonly List<IAIAction> _actions;
        private readonly EnemyAIDefinition _brainDef;
        private readonly TargetingDefinition _targeting;
        private readonly float _thinkInterval;
        private UnitInstance _unit;

        private readonly AIContext _ctx;
        private readonly EnemyBlackboard _blackboard;

        private float _nextThinkTime;

        private const float ScoreEpsilon = 0.0001f;
        private const float MaxWeight = 5f;

        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public EnemyBlackboard Blackboard => _blackboard;

#if UNITY_EDITOR
        public bool DebugLog;
        private readonly StringBuilder _logBuilder = new();
#endif

        public UtilityUnitBrain(
            List<IAIAction> actions,
            EnemyAIDefinition brainDefinition,
            TargetingDefinition targetingDefinition,
            EnemyBlackboard blackboard)
        {
            _actions = actions;
            _brainDef = brainDefinition;
            _targeting = targetingDefinition;
            _thinkInterval = brainDefinition.ThinkInterval > 0f ? brainDefinition.ThinkInterval : 0.2f;
            _blackboard = blackboard;
            _ctx = new AIContext();
        }

        public void Initialize(UnitInstance unit) => _unit = unit;

        public void Tick(float dt)
        {
            if (!_isEnabled) return;
            _nextThinkTime -= dt;
            if (_nextThinkTime > 0f) return;
            _nextThinkTime += _thinkInterval;
            Think(dt);
        }

        private void Think(float dt)
        {
            AIContextBuilder.Fill(_unit, dt, _ctx, _blackboard, _targeting);
            

            var state = _ctx.CurrentState;
            if (state == UnitStateId.Suppressed ||
                state == UnitStateId.Dying ||
                state == UnitStateId.Dead)
                return;

            IAIAction best = null;
            ActionDecision bestRawDecision = ActionDecision.Zero; // Execute получает это
            float bestWeightedScore = float.MinValue; // arbitration по этому
            int bestIndex = int.MaxValue;

#if UNITY_EDITOR
            bool log = DebugLog;
            if (log) _logBuilder.Clear().AppendLine($"[UtilityAI] {_unit.name} ──────────────");
#endif

            for (int i = 0; i < _actions.Count; i++)
            {
                var action = _actions[i];

                // ── Weight lookup ──────────────────────────────────────
                // Builder гарантирует что все action'ы присутствуют в словаре.
                // TryGetAction вернёт false только если action добавлен в код
                // но не добавлен в EnemyAIDefinitionBuilder.FillMissingWithDefaults().
                // Это баг — логируем и пропускаем, не фолбэкаем молча.
                if (!_brainDef.TryGetAction(action.Type, out var actionDef))
                {
                    Debug.LogError(
                        $"[UtilityAI] {_unit.name}: action {action.Type} not found in brain definition. " +
                        $"Add it to EnemyAIDefinitionBuilder.FillMissingWithDefaults().");
                    continue;
                }

                if (!actionDef.Enabled) continue;

                // ── Raw score ──────────────────────────────────────────
                var rawDecision = action.Evaluate(_unit, _ctx, _blackboard);

#if UNITY_EDITOR
                if (rawDecision.Score < 0f || rawDecision.Score > 1f)
                    Debug.LogWarning(
                        $"[UtilityAI] {action.GetType().Name} raw={rawDecision.Score:F2} out of [0,1]");
#endif

                // ── Weighted arbitration ───────────────────────────────
                // Weight используется ТОЛЬКО для выбора победителя.
                // Execute() получает RAW decision — без weight-мутации.
                float weight = Mathf.Clamp(actionDef.Weight, 0f, MaxWeight);
                float weightedScore = rawDecision.Score * weight; 

#if UNITY_EDITOR
                if (log)
                    _logBuilder.AppendLine(
                        $"  {action.Type,-12} raw={rawDecision.Score:F2} w={weight:F2} weighted={weightedScore:F2}");
#endif

                // ── Select best ────────────────────────────────────────
                bool betterScore = weightedScore > bestWeightedScore + ScoreEpsilon;
                bool tiedButHigherPriority = Mathf.Abs(weightedScore - bestWeightedScore) <= ScoreEpsilon
                                             && i < bestIndex;

                if (betterScore || tiedButHigherPriority)
                {
                    bestRawDecision = rawDecision; // сохраняем raw
                    bestWeightedScore = weightedScore;
                    best = action;
                    bestIndex = i;
                }
            }

#if UNITY_EDITOR
            if (log && best != null)
            {
                _logBuilder.AppendLine(
                    $"  WINNER: {best.Type} weighted={bestWeightedScore:F2} raw={bestRawDecision.Score:F2}");
                Debug.Log(_logBuilder.ToString());
            }
#endif

            // Execute получает RAW decision — semantics не сломаны
            best?.Execute(_unit, _ctx, _blackboard, bestRawDecision);
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
            foreach (var action in _actions) action.Dispose();
            _actions.Clear();
        }
    }
}
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Units.Brain.Utility.Core;
using Galactic1.Game.Meta.Enemy;

namespace Galactic1.Code.Systems.Raid.Enemies
{
    /// <summary>
    /// Immutable runtime AI brain settings.
    ///
    /// Добавлено:
    ///   _actions — Dictionary по AIActionType.
    ///   TryGetAction() — lookup для Brain think-loop.
    ///
    /// Строится в ZombieRuntimeFactory через EnemyAIDefinitionBuilder.
    /// </summary>
    public sealed class EnemyAIDefinition
    {
        public float ThinkInterval  { get; }
        public float RoamRadius     { get; }
        public float WaypointRadius { get; }
        public bool  UsePackBehaviour { get; }

        private readonly Dictionary<AIActionType, AIActionDefinition> _actions;

        public EnemyAIDefinition(
            float thinkInterval,
            float roamRadius,
            float waypointRadius,
            bool  usePackBehaviour,
            Dictionary<AIActionType, AIActionDefinition> actions)
        {
            ThinkInterval    = thinkInterval;
            RoamRadius       = roamRadius;
            WaypointRadius   = waypointRadius;
            UsePackBehaviour = usePackBehaviour;
            _actions         = actions ?? new Dictionary<AIActionType, AIActionDefinition>();
        }

        /// <summary>
        /// Возвращает runtime definition для action type.
        /// false если action не задан в конфиге — Brain должен пропустить его.
        /// </summary>
        public bool TryGetAction(AIActionType type, out AIActionDefinition def)
            => _actions.TryGetValue(type, out def);

        /// <summary>
        /// true если action задан и enabled.
        /// Shortcut для Brain.
        /// </summary>
        public bool IsActionEnabled(AIActionType type)
            => _actions.TryGetValue(type, out var def) && def.Enabled;
    }
}
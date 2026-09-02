
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Game.Meta.Enemy;

namespace Galactic1.Code.Systems.Raid
{
    /// <summary>
    /// Runtime-контракт для врагов (зомби, боссы).
    /// Расширяет IUnitRuntimeBase только тем, что уникально для AI-юнитов.
    /// </summary>
    public interface IEnemyUnitRuntime : IUnitRuntimeBase
    {
        /// <summary>
        /// ID конфига врага (ссылка на EnemyConfig ScriptableObject).
        /// Используется спавнером и лут-системой.
        /// </summary>
        EnemyId EnemyId { get; }

        EnemyRuntimeDefinition Definition { get; }
        /// <summary>
        /// Уровень угрозы [0..1] — для приоритизации целей игроком
        /// и систем сложности рейда.
        /// </summary>
        float ThreatLevel { get; }

        EnemyAIProfile AIProfile { get;}
    }
}
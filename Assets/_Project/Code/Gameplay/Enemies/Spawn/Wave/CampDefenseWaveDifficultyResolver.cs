using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Waves
{
    /// <summary>Итог резолва сложности волн Camp Defense для конкретного уровня игрока.</summary>
    public readonly struct WaveDifficultyResolution
    {
        public readonly float Scale;
        public readonly IReadOnlyList<EnemyId> ExcludedEnemyIds;

        public WaveDifficultyResolution(float scale, IReadOnlyList<EnemyId> excludedEnemyIds)
        {
            Scale = scale;
            ExcludedEnemyIds = excludedEnemyIds;
        }

        public static WaveDifficultyResolution Default { get; } =
            new(1f, System.Array.Empty<EnemyId>());
    }

    /// <summary>
    /// Резолвит множитель количества врагов и список исключённых типов Camp Defense
    /// из уровня игрока.
    ///
    /// НЕ хранит состояние между вызовами, НЕ знает про ProgressionService —
    /// playerLevel передаётся снаружи (CampDefenseScenario.OnSceneLoaded()).
    /// Единственная ответственность — маппинг level → (WaveScale, ExcludedEnemyIds)
    /// через WaveConfig.DifficultyRules.
    /// </summary>
    public sealed class CampDefenseWaveDifficultyResolver
    {
        private readonly WaveConfig _config;

        public CampDefenseWaveDifficultyResolver(WaveConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Первое правило (по возрастанию MaxPlayerLevel), для которого
        /// playerLevel &lt;= rule.MaxPlayerLevel. Нет правил / ни одно не подошло →
        /// scale 1f, без исключений. Scale всегда clamped в [0, 1].
        /// </summary>
        public WaveDifficultyResolution Resolve(int playerLevel)
        {
            if (_config == null || _config.DifficultyRules == null || _config.DifficultyRules.Count == 0)
                return WaveDifficultyResolution.Default;

            int level = Mathf.Max(playerLevel, 0);

            var rules = _config.DifficultyRules;
            for (int i = 0; i < rules.Count; i++)
            {
                if (level <= rules[i].MaxPlayerLevel)
                {
                    return new WaveDifficultyResolution(
                        Mathf.Clamp01(rules[i].WaveScale),
                        rules[i].ExcludedEnemyIds);
                }
            }

            return WaveDifficultyResolution.Default;
        }
    }
}
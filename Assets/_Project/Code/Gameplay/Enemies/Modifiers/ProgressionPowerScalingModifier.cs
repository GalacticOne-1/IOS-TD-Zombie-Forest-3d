using System.Collections.Generic;
using Galactic1.Code.Gameplay.Enemies.Spawning;
using Galactic1.Code.Systems.Progression;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Game.Meta.Enemy.Modifiers;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Modifiers
{
    /// <summary>
    /// Глобальный шаг масштабирования силы врагов по уровню прогрессии игрока.
    ///
    /// НЕ регистрируется в EnemyModifierDatabase и не зависит от Request.ModifierIds.
    /// Вызывается EnemyModifierPipeline ПОСЛЕ всех явных модификаторов.
    /// Сейчас масштабирует только StatId.Damage.
    ///
    /// Уровень читается из ProgressionService (единственный источник) в момент спавна.
    /// Сцен-объектов не использует.
    /// </summary>
    public sealed class ProgressionPowerScalingModifier : IEnemyModifier
    {
        public const string Id = "__progression_power_scaling";
        public string ModifierId => Id;

        // Чтобы масштабировать другие статы позже — добавить StatId сюда.
        private static readonly StatId[] ScaledStats = { StatId.Damage };

        private readonly struct LevelRange
        {
            public readonly int Min;
            public readonly int Max;
            public readonly float Multiplier;

            public LevelRange(int min, int max, float multiplier)
            {
                Min = min;
                Max = max;
                Multiplier = multiplier;
            }
        }

        private readonly ProgressionService _progression;
        private readonly LevelRange[] _ranges;
        private readonly HashSet<int> _warnedLevels = new();
        
        
        /// <summary>Текущий множитель для уровня игрока (для UI/логов).</summary>
        public float CurrentMultiplier =>
            _progression == null ? 1f : ResolveMultiplier(_progression.CurrentLevel);
        
        
        
        
        

        public ProgressionPowerScalingModifier(
            ProgressionService progression,
            EnemyPowerScalingConfig config)
        {
            _progression = progression;

            var list = new List<LevelRange>();
            if (config == null)
            {
                Debug.LogWarning("[ProgressionPowerScaling] Config == null — множитель всегда 1.0.");
            }
            else
            {
                var entries = config.Entries;
                for (int i = 0; i < entries.Count; i++)
                {
                    var e = entries[i];
                    if (e == null
                        || e.MinPlayerLevel > e.MaxPlayerLevel
                        || float.IsNaN(e.PowerMultiplier)
                        || e.PowerMultiplier < 0f)
                    {
                        Debug.LogError($"[ProgressionPowerScaling] Entry {i} невалидна — пропущена.");
                        continue;
                    }

                    list.Add(new LevelRange(e.MinPlayerLevel, e.MaxPlayerLevel, e.PowerMultiplier));
                }
            }

            list.Sort((a, b) => a.Min.CompareTo(b.Min));
            _ranges = list.ToArray();
        }

        public void Apply(EnemySpawnContext context)
        {
            var mutation = context.MutationContext;
            if (mutation == null || _progression == null) return;

            float multiplier = ResolveMultiplier(_progression.CurrentLevel);
            if (Mathf.Approximately(multiplier, 1f)) return;

            foreach (var stat in ScaledStats)
                mutation.MultiplyStatIfExists(stat, multiplier);
        }

        private float ResolveMultiplier(int level)
        {
            for (int i = 0; i < _ranges.Length; i++)
            {
                var r = _ranges[i];
                if (level >= r.Min && level <= r.Max)
                    return r.Multiplier;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_warnedLevels.Add(level))
                Debug.LogWarning($"[ProgressionPowerScaling] Уровень {level} не покрыт конфигом — множитель 1.0.");
#endif
            return 1f;
        }
        
        
        
        
        
        /// <summary>Пишет в консоль уровень игрока и процент силы врагов.</summary>
        public void LogCurrent(string source)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_progression == null)
            {
                Debug.LogWarning($"[EnemyPowerScaling] {source}: ProgressionService == null — сила 100%.");
                return;
            }

            int level = _progression.CurrentLevel;
            float m = ResolveMultiplier(level);
            DLog.Alert($"[EnemyPowerScaling] {source} | Уровень игрока = {level} | " +
                      $"Сила врагов = {m * 100f:0.#}% (урон ×{m:0.##})", EDlogColor.YELLOW);
#endif
        }
    }
}

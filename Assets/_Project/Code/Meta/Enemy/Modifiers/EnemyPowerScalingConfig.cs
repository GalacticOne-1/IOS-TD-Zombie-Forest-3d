using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Game.Meta.Enemy.Modifiers
{
    [Serializable]
    public sealed class EnemyPowerScalingEntry
    {
        public int MinPlayerLevel = 1;
        public int MaxPlayerLevel = 1;

        [Range(0f, 2f)]
        public float PowerMultiplier = 1f;
    }

    /// <summary>
    /// Глобальное правило: уровень прогрессии игрока → множитель силы врагов.
    /// Только authoring-данные: уровень не читает, поведения не содержит.
    /// Сейчас множитель применяется только к StatId.Damage
    /// (см. ProgressionPowerScalingModifier).
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnemyPowerScaling",
        menuName = "Game Configs/Enemy/Enemy Power Scaling")]
    public sealed class EnemyPowerScalingConfig : ScriptableObject
    {
        [SerializeField] private List<EnemyPowerScalingEntry> entries = new();

        public IReadOnlyList<EnemyPowerScalingEntry> Entries => entries;

        /// <summary>Собирает человекочитаемые проблемы конфигурации.</summary>
        public void Validate(List<string> problems)
        {
            if (entries == null || entries.Count == 0)
            {
                problems.Add("Нет ни одной записи — множитель всегда будет 1.0.");
                return;
            }

            var sorted = new List<EnemyPowerScalingEntry>();
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null)
                {
                    problems.Add($"Entry {i}: null.");
                    continue;
                }

                if (e.MinPlayerLevel < 0 || e.MaxPlayerLevel < 0)
                    problems.Add($"Entry {i}: отрицательный уровень.");
                if (e.MinPlayerLevel > e.MaxPlayerLevel)
                    problems.Add($"Entry {i}: Min ({e.MinPlayerLevel}) > Max ({e.MaxPlayerLevel}).");
                if (float.IsNaN(e.PowerMultiplier) || e.PowerMultiplier < 0f || e.PowerMultiplier > 2f)
                    problems.Add($"Entry {i}: множитель {e.PowerMultiplier} вне диапазона 0..2.");

                sorted.Add(e);
            }

            sorted.Sort((a, b) => a.MinPlayerLevel.CompareTo(b.MinPlayerLevel));

            if (sorted.Count > 0 && sorted[0].MinPlayerLevel > 1)
                problems.Add($"Уровни 1..{sorted[0].MinPlayerLevel - 1} не покрыты (будет 1.0).");

            for (int i = 1; i < sorted.Count; i++)
            {
                var prev = sorted[i - 1];
                var cur = sorted[i];

                if (cur.MinPlayerLevel <= prev.MaxPlayerLevel)
                    problems.Add($"Пересечение: [{prev.MinPlayerLevel}..{prev.MaxPlayerLevel}] и " +
                                 $"[{cur.MinPlayerLevel}..{cur.MaxPlayerLevel}].");
                else if (cur.MinPlayerLevel > prev.MaxPlayerLevel + 1)
                    problems.Add($"Дыра: уровни {prev.MaxPlayerLevel + 1}..{cur.MinPlayerLevel - 1} " +
                                 "не покрыты (будет 1.0).");
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var problems = new List<string>();
            Validate(problems);
            foreach (var p in problems)
                Debug.LogWarning($"[EnemyPowerScalingConfig] '{name}': {p}", this);
        }
#endif
    }
}

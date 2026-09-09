using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Waves
{
    [Serializable]
    public sealed class WaveDifficultyRule
    {
        [Min(1)]
        public int MaxPlayerLevel;

        [Range(0.01f, 1f)]
        public float WaveScale = 1f;

        [Tooltip("Типы врагов, которые НЕ должны спавниться вовсе для игроков этого диапазона уровней " +
                 "(например, элитные/опасные типы, отключённые для низких уровней в Soft Launch).")]
        public List<EnemyId> ExcludedEnemyIds = new();
    }

    /// <summary>Упорядоченный список волн для одной локации Camp Defense.</summary>
    [CreateAssetMenu(
        fileName = "WaveConfig",
        menuName = "Game Configs/Enemy/Wave Config")]
    public sealed class WaveConfig : ScriptableObject
    {
        public List<WaveDefinition> Waves = new();

        [Tooltip("Правила снижения количества врагов Camp Defense по уровню игрока (Soft Launch). " +
                 "Оцениваются по возрастанию MaxPlayerLevel — берётся первое правило, где playerLevel <= MaxPlayerLevel.")]
        public List<WaveDifficultyRule> DifficultyRules = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 1; i < DifficultyRules.Count; i++)
            {
                if (DifficultyRules[i].MaxPlayerLevel < DifficultyRules[i - 1].MaxPlayerLevel)
                {
                    Debug.LogWarning(
                        $"[WaveConfig] DifficultyRules не отсортированы по возрастанию MaxPlayerLevel " +
                        $"(индекс {i - 1} → {i}). CampDefenseWaveDifficultyResolver ожидает восходящий порядок.");
                    break;
                }
            }
        }
#endif
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Systems.Progression
{
    /// <summary>
    /// Data-driven progression curve.
    /// Each entry says how much *total accumulated* XP is required to reach
    /// that level. Level 1 is the starting level (index 0, RequiredXP is
    /// ignored/should be 0).
    /// </summary>
    [CreateAssetMenu(fileName = "ProgressionDefinition", menuName = "Game Configs/Progression/Progression Definition")]
    public sealed class ProgressionDefinition : ScriptableObject
    {
        [Serializable]
        public class LevelEntry
        {
            [Tooltip("1-based level number")]
            public int Level = 1;

            [Tooltip("Total accumulated XP required to reach this level")]
            public int RequiredXP;
        }

        [SerializeField] private List<LevelEntry> levels = new();

        public int MaxLevel
        {
            get
            {
                int max = 1;
                foreach (var entry in levels)
                    if (entry.Level > max)
                        max = entry.Level;
                return max;
            }
        }

        /// <summary>
        /// Returns the highest level whose RequiredXP threshold is met by totalXP.
        /// Correctly resolves multi-level jumps (one large XP grant crossing
        /// several thresholds at once) — no assumption of a +1 step anywhere.
        /// </summary>
        public int GetLevelForXP(int totalXP)
        {
            int result = 1;

            foreach (var entry in levels)
            {
                if (totalXP >= entry.RequiredXP && entry.Level > result)
                    result = entry.Level;
            }

            return result;
        }

        public bool TryGetRequiredXP(int level, out int requiredXP)
        {
            foreach (var entry in levels)
            {
                if (entry.Level == level)
                {
                    requiredXP = entry.RequiredXP;
                    return true;
                }
            }

            requiredXP = 0;
            return false;
        }

        /// <summary>
        /// Total XP threshold at the start of the given level (0 if level 1
        /// or not found — level 1 always starts at 0 cumulative XP).
        /// </summary>
        public int GetLevelStartXP(int level)
            => TryGetRequiredXP(level, out var xp) ? xp : 0;

        /// <summary>
        /// Total XP threshold required for currentLevel + 1.
        /// Returns false at MaxLevel — caller must treat that as "no next level"
        /// rather than dividing by a zero-width span.
        /// </summary>
        public bool TryGetNextLevelXP(int currentLevel, out int nextLevelXP)
            => TryGetRequiredXP(currentLevel + 1, out nextLevelXP);
    }
}
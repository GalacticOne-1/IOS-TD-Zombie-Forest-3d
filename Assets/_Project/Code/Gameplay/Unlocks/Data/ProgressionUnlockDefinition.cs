using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Systems.Progression
{
    /// <summary>
    /// Data-driven unlock rules: UnlockId → requirements (AND'ed together).
    ///
    /// For Soft Launch every entry will hold a single ProgressionLevelRequirement,
    /// but the list supports combining multiple requirement types once they exist,
    /// without any change to UnlockService/ProgressionUnlockService.
    /// </summary>
    [CreateAssetMenu(fileName = "ProgressionUnlockDefinition", 
        menuName = "Game Configs/Progression/Progression Unlock Definition")]
    public sealed class ProgressionUnlockDefinition : ScriptableObject
    {
        [Serializable]
        public class UnlockEntry
        {
            public UnlockId Id;

            [SerializeReference]
            public List<UnlockRequirement> Requirements = new();
        }

        [SerializeField] private List<UnlockEntry> entries = new();

        public IReadOnlyList<UnlockEntry> Entries => entries;

        public bool IsSatisfied(UnlockEntry entry, IUnlockContext context)
        {
            foreach (var requirement in entry.Requirements)
            {
                if (requirement == null || !requirement.IsMet(context))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Editor/UI convenience only — NOT used for gating logic.
        /// Lets UI show "Requires Level X" without duplicating the number.
        /// </summary>
        public bool TryGetDisplayRequiredLevel(UnlockId id, out int level)
        {
            foreach (var entry in entries)
            {
                if (entry.Id != id)
                    continue;

                foreach (var requirement in entry.Requirements)
                {
                    if (requirement is ProgressionLevelRequirement levelReq)
                    {
                        level = levelReq.RequiredLevel;
                        return true;
                    }
                }
            }

            level = 0;
            return false;
        }
    }
}

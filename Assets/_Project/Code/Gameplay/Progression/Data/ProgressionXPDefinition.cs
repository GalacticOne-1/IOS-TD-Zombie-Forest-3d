using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Systems.Progression
{
    /// <summary>
    /// Data-driven XP reward table.
    ///
    /// Rewards are grouped by ProgressionActionId (category — "Zombie Kill",
    /// "Loot", "Location", "Facility") and, inside each category, keyed by the
    /// entity's own existing RuntimeId (EnemyId, LootContainerId, LocationId,
    /// ItemId for facilities). This lets every entity have its own value from
    /// day one without any code change — Soft Launch just happens to use the
    /// same number for every entry in a category.
    ///
    /// ProgressionXPService never touches this dictionary directly for
    /// resolution — it calls the intent-named Get*XP helpers below, so the
    /// category concept stays entirely inside this asset.
    /// </summary>
    [CreateAssetMenu(fileName = "ProgressionXPDefinition", 
        menuName = "Game Configs/Progression/Progression XP Definition")]
    public sealed class ProgressionXPDefinition : ScriptableObject
    {
        [Header("Categories")]
        [SerializeField] private ProgressionActionId zombieKillCategory;
        [SerializeField] private ProgressionActionId lootCategory;
        [SerializeField] private ProgressionActionId locationCategory;
        [SerializeField] private ProgressionActionId facilityCategory;

        [Serializable]
        public class SourceEntry
        {
            [Tooltip("The entity's own existing RuntimeId (EnemyId / LootContainerId / LocationId / ItemId)")]
            public RuntimeId SourceId;
            public int XPAmount;
        }

        [Serializable]
        public class CategoryEntry
        {
            public ProgressionActionId ActionId;
            public List<SourceEntry> Sources = new();
        }

        [Header("Rewards")]
        [SerializeField] private List<CategoryEntry> categories = new();

        public int GetZombieKillXP(RuntimeId enemyId) => GetXP(zombieKillCategory, enemyId);
        public int GetLootXP(RuntimeId containerId) => GetXP(lootCategory, containerId);
        public int GetLocationXP(RuntimeId locationId) => GetXP(locationCategory, locationId);
        public int GetFacilityXP(RuntimeId facilityId) => GetXP(facilityCategory, facilityId);

        private int GetXP(ProgressionActionId category, RuntimeId sourceId)
        {
            if (category == null || sourceId == null)
                return 0;

            foreach (var cat in categories)
            {
                if (cat.ActionId != category)
                    continue;

                foreach (var source in cat.Sources)
                {
                    if (source.SourceId == sourceId)
                        return source.XPAmount;
                }

                Debug.LogWarning($"[ProgressionXPDefinition] No XP entry for source '{sourceId.DebugKey}' " +
                                  $"in category '{category.DebugKey}'.");
                return 0;
            }

            Debug.LogWarning($"[ProgressionXPDefinition] No category entry for '{category?.DebugKey}'.");
            return 0;
        }
    }
}

using Galactic1.RaidLoot.Authoring;
using UnityEngine;

namespace Galactic1.Code.WorldMap.Intel
{
    
    /// <summary>
    /// Набор разведданных о локации.
    /// Используется для UI, баланса и принятия решений игроком.
    /// Может быть частично неизвестен (fog of war).
    /// </summary>
    [System.Serializable]
    public struct LocationIntel
    {
        public LocationThreatLevel threatLevel;
        public EnemyCompositionType enemyType;
        public EnvironmentalHazardType hazardType;
        public OperationalRiskLevel riskLevel;
        
        [Header("Rewards")]
        public LocationGuaranteedProfileConfig guaranteedLootProfile;
        public LocationLootProfileConfig lootProfile;
        public LootProfileType loot;
        
        public LocationResourceVolume[] resourcesVolume;    // разделение по категориям объема ресурса



        
        /// <summary>
        /// true - предмет есть в локации
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool HasCategory(LootEconomyCategory category)
        {
            var l = resourcesVolume.Length;
            for (int i = 0; i < l; i++)
            {
                var s = resourcesVolume[i].lootEconomyCategory.Length;
                for (int j = 0; j < s; j++)
                {
                    if (resourcesVolume[i].lootEconomyCategory[j] == category)
                        return true;
                }
            }

            return false;
        }
    }
}
using System;
using Galactic1.RaidLoot.Authoring;

namespace Galactic1.Code.WorldMap.Intel
{
    /// <summary>
    /// Разведданные по конкретному типу ресурса в локации.
    /// </summary>
    [Serializable]
    public struct LocationResourceVolume
    {
        public ResourceVolume volume;
        public LootEconomyCategory[] lootEconomyCategory;
    }
}

using UnityEngine;

namespace Galactic1.RaidLoot.Authoring
{
    [CreateAssetMenu(
        fileName = "LocationLootProfileConfig",
        menuName = "Game Configs/Loot/Location Loot Profile Config")]
    public class LocationLootProfileConfig : ScriptableObject
    {
        [SerializeField] private LootMultiplierEntry[] multipliers;

        public LootMultiplierEntry[] Multipliers => multipliers;
    }
}
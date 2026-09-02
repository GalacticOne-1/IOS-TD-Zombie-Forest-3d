
using UnityEngine;

namespace Galactic1.RaidLoot.Authoring
{
    [CreateAssetMenu(
        fileName = "LootPoolConfig",
        menuName = "Game Configs/Loot/Loot Pool Config")]
    public sealed class LootPoolConfig : ScriptableObject
    {
        [SerializeField] private LootWeightedEntry[] _pool;

        public LootWeightedEntry[] Pool => _pool;
    }
}
using UnityEngine;

namespace Galactic1.Core.Systems.Factories
{
    /// <summary>
    /// Spawns loot items when enemies die or objects break.
    /// Uses LootConfig with drop tables (LDoE-style loot system).
    /// </summary>
    // [CreateAssetMenu(menuName = "Game Configs/Factories/Loot Factory")]
    // public class LootFactory : BaseFactory<LootItem>
    // {
    //     public LootItem Create(LootConfig config, Vector3 pos)
    //     {
    //         var loot = Instantiate(config.itemPrefab, pos, Quaternion.identity);
    //         loot.Initialize(config);
    //         return loot;
    //     }
    //
    //     public override LootItem Create(Vector3 position, Quaternion rotation)
    //     {
    //         return Instantiate(Resources.Load<LootItem>("DefaultLoot"), position, rotation);
    //     }
    // }
}
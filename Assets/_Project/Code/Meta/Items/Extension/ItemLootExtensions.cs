using Galactic1.Core.Enums;
using Galactic1.Gameplay;
using Galactic1.RaidLoot.Authoring;

namespace Galactic1.Game.Meta.Items
{
    public static class ItemLootExtensions
    {
        public static LootModule Loot(this ItemConfig item)
        {
            return item.GetModule<LootModule>();
        }

        public static int LootValue(this ItemConfig item)
        {
            return item.GetModule<LootModule>()?.LootCost ?? 1;
        }

        public static Tier LootTier(this ItemConfig item)
        {
            return item.Classification.tier;
        }

        public static LootDropTag LootTag(this ItemConfig item)
        {
            return item.GetModule<LootModule>()?.DropTag ?? LootDropTag.Generic;
        }
    }
}
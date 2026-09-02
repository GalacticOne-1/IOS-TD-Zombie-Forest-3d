using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.CampDefense.Penalty
{
    /// <summary>
    /// Immutable DTO — сколько единиц конкретного предмета будет изъято.
    /// Никакой логики, только данные.
    /// Item хранится готовым (аналогично RaidRewardLootData.Item) — чтобы
    /// CampDefensePenaltyResultMapper не делал повторный резолв по ItemId.
    /// </summary>
    public readonly struct CampDefensePenaltyItem
    {
        public RuntimeId ItemId { get; }
        public ItemConfig Item { get; }
        public int Amount { get; }

        public CampDefensePenaltyItem(ItemConfig item, int amount)
        {
            Item = item;
            ItemId = item.Id;
            Amount = amount;
        }
    }
}
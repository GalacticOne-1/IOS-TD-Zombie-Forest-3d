using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Core
{
    /// <summary>
    /// Сериализуемый DTO одного ресурса, потерянного при поражении Camp Defense.
    /// Аналог RaidRewardLootData, но без Durability/AmmoInMagazine —
    /// изымаются только ItemLabel.Resource предметы, у них их нет.
    /// </summary>
    [System.Serializable]
    public class RaidPenaltyLossData
    {
        public int Id { get; set; }
        public string ConfigId { get; set; }
        public int Amount { get; set; }
        public ItemConfig Item { get; set; }
    }
}
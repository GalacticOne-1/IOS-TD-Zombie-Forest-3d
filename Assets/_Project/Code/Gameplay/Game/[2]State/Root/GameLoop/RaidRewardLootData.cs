using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Core
{
    [System.Serializable]
    public class RaidRewardLootData
    {
        public int Id { get; set; }
        public string ConfigId { get; set; }
        public int Amount { get; set; }
        public int Durability { get; set; }
        public int AmmoInMagazine { get; set; }
        public ItemConfig Item { get; set; }
    }
}
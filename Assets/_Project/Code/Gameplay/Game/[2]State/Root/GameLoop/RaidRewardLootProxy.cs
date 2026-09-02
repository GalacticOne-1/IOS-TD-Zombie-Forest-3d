using R3;

namespace Galactic1.Code.Core
{
    public class RaidRewardLootProxy
    {
        public readonly RaidRewardLootData Origin;
        
        public int Id => Origin.Id;
        public readonly ReactiveProperty<string> ConfigId;
        public readonly ReactiveProperty<int> Amount;
        public readonly ReactiveProperty<int> Durability;
        public readonly ReactiveProperty<int> AmmoInMagazine;
        
        public RaidRewardLootProxy(RaidRewardLootData data)
        {
            Origin = data;
            
            ConfigId = new(Origin.ConfigId);
            Amount = new(Origin.Amount);
            Durability = new(Origin.Durability);
            AmmoInMagazine = new(Origin.AmmoInMagazine);
            
            // Перепривязываем Item/Amount
            BindToSave(data);
        }
        
        
        public void BindToSave(RaidRewardLootData origin)
        {
            ConfigId.Skip(1).Subscribe(_ => origin.ConfigId = _);
            Amount.Skip(1).Subscribe(_ => origin.Amount = _);
            Durability.Skip(1).Subscribe(_ => origin.Durability = _);
            AmmoInMagazine.Skip(1).Subscribe(_ => origin.AmmoInMagazine = _);
        }
    }
}
using System.Linq;
using ObservableCollections;
using R3;

namespace Galactic1.Code.Core
{
    
    public class RaidResultProxy
    {
        private readonly RaidResultData Origin;
        
        public readonly ReactiveProperty<bool> IsSuccess;
        public readonly ReactiveProperty<int> KilledEnemies;
        public readonly ReactiveProperty<int> ExperienceGained;
        public readonly ReactiveProperty<bool> MainBuildingDestroyed;


        public ObservableList<RaidRewardLootProxy> LootReceived  { get; } = new();
        public ObservableList<RaidPenaltyLossProxy> ResourcesLost { get; } = new();


        public RaidResultProxy(RaidResultData data)
        {
            Origin = data;
            
            IsSuccess = new(Origin.IsSuccess);
            IsSuccess.Skip(1).Subscribe(_ => Origin.IsSuccess = _);
            KilledEnemies = new(Origin.KilledEnemies);
            KilledEnemies.Skip(1).Subscribe(_ => Origin.KilledEnemies = _);
            ExperienceGained = new(Origin.ExperienceGained);
            ExperienceGained.Skip(1).Subscribe(_ => Origin.ExperienceGained = _);
            
            MainBuildingDestroyed = new(Origin.MainBuildingDestroyed);
            MainBuildingDestroyed.Skip(1).Subscribe(_ => Origin.MainBuildingDestroyed = _);
            
            InitializeLoot();
            InitializeResourcesLost();
        }

        
        void InitializeLoot()
        {
            Origin.LootReceived.ForEach(lootData => LootReceived.Add(new RaidRewardLootProxy(lootData)));
            
            // при добавлении связываем с сохранением
            LootReceived.ObserveAdd().Subscribe(e =>
            {
                Origin.LootReceived.Add(e.Value.Origin);
            });
            
            // так же при удалении удаляем сохранение
            LootReceived.ObserveRemove().Subscribe(e =>
            {
                Origin.LootReceived.Remove(Origin.LootReceived.FirstOrDefault(r => r.Id == e.Value.Id));
            });
            
            LootReceived.ObserveReplace().Subscribe(e =>
            {
                Origin.LootReceived[e.Index] = e.NewValue.Origin;
                e.NewValue.BindToSave(e.NewValue.Origin);
            });
            
        }

        void InitializeResourcesLost()
        {
            Origin.ResourcesLost.ForEach(lossData => ResourcesLost.Add(new RaidPenaltyLossProxy(lossData)));

            ResourcesLost.ObserveAdd().Subscribe(e =>
            {
                Origin.ResourcesLost.Add(e.Value.Origin);
            });

            ResourcesLost.ObserveRemove().Subscribe(e =>
            {
                Origin.ResourcesLost.Remove(Origin.ResourcesLost.FirstOrDefault(r => r.Id == e.Value.Id));
            });

            ResourcesLost.ObserveReplace().Subscribe(e =>
            {
                Origin.ResourcesLost[e.Index] = e.NewValue.Origin;
                e.NewValue.BindToSave(e.NewValue.Origin);
            });
        }
        
    }
}
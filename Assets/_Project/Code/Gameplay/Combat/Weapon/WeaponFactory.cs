using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Weapons.Infrastructure
{
    public sealed class WeaponFactory
    {
        private readonly IAmmoInventory _inventory;
        private readonly IOwnerStatsProvider _stats;

        public WeaponFactory(IAmmoInventory inventory, IOwnerStatsProvider stats)
        {
            _inventory = inventory;
            _stats = stats;
        }

        public WeaponEntity Create(
            WeaponModule module,
            WeaponDefinition weaponDef, 
            IWeaponInventorySync inventorySync)
        {
            var data = weaponDef.ToData();
            var entity = new WeaponEntity(module, data, inventorySync);

            // Обязательные компоненты — есть у всех
            entity.AddComponent(new FireComponent());
            entity.AddComponent(new DurabilityComponent());
            entity.AddComponent(new AmmoComponent(_inventory));
            entity.AddComponent(new ReloadComponent());
            entity.AddComponent(new SpreadComponent(_stats));
            entity.AddComponent(new DamageFalloffComponent());

            // Опциональные — только если нужны по конфигу
            if (data.HasHeat) entity.AddComponent(new HeatComponent());
            if (data.HasSuppression) entity.AddComponent(new SuppressComponent());

            return entity;
        }
    }
}
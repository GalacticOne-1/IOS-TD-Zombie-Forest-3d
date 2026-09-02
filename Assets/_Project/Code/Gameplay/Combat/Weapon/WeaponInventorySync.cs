using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.Gameplay.Weapons.Infrastructure
{
    /// <summary>
    /// Конкретная привязка WeaponEntity к слоту инвентаря.
    /// </summary>
    public sealed class WeaponInventorySync : IWeaponInventorySync
    {
        private readonly IInventorySource _source;
        private readonly EquipmentRuntimeService _equipment;
        private readonly int _slotIndex;
        private WeaponEntity _entity;

        public WeaponInventorySync(
            IInventorySource source,
            EquipmentRuntimeService equipment,
            int slotIndex)
        {
            _source = source;
            _equipment = equipment;
            _slotIndex = slotIndex;
        }
        
        public void Bind(WeaponEntity entity)
        {
            _entity = entity;
            entity.OnAmmoChanged += OnAmmoChanged;
            entity.OnDurabilityChanged += OnDurabilityChanged;
        }
        public void Unbind(WeaponEntity entity)
        {
            entity.OnAmmoChanged -= OnAmmoChanged;
            entity.OnDurabilityChanged -= OnDurabilityChanged;
        }


        private void OnAmmoChanged(int current, int max)
        {
            var slot = _source.GetSlot(_slotIndex);
            if (slot.IsEmpty) return;

            slot.AmmoInMagazine = current;
            _source.SetSlot(_slotIndex, slot);
            
        }
        
        private void OnDurabilityChanged(int current, int max)
        {
            var slot = _source.GetSlot(_slotIndex);
            if (slot.IsEmpty) return;
            
            _equipment.OnItemUsed(_slotIndex, current);
        }

    }
}
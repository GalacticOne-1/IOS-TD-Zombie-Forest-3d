using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment.Snapshots;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Meta.Stats;
using Galactic1.Items;

namespace Galactic1.Code.Gameplay.Equipment
{
    /// <summary>
    /// Источник стат-модификаторов экипировки для runtime.
    /// НЕ связан со сценой.
    /// </summary>
    public interface IEquipmentStatsProvider
    {
        IInventorySource Source { get; }
        event Action<EquipSlotType, ItemConfig> OnEquipped;
        event Action<EquipSlotType> OnUnequipped;
        event Action OnClearAll;
        event Action<ItemConfig> OnItemBroken;
        event Action OnUpdate;

        void RestoreEquipmentFromInventory();
        IReadOnlyList<ItemStatEntry> GetEquippedModifiers();
        EquipmentSnapshot CreateReadonlySnapshot();
        
        /// <summary>
        /// Применить урон к прочности ВСЕЙ экипировки
        /// </summary>
        void ApplyDurabilityDamage(float damage);
    }
}
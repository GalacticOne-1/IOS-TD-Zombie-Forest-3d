using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment.Snapshots;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Meta.Stats;

namespace Galactic1.Code.Gameplay.Equipment
{
    /// <summary>
    /// Пустой provider для врагов/NPC без экипировки.
    /// </summary>
    public sealed class EmptyEquipmentStatsProvider : IEquipmentStatsProvider
    {
        public static readonly EmptyEquipmentStatsProvider Instance = new();

        public IInventorySource Source { get; }
        public event Action<EquipSlotType, ItemConfig> OnEquipped;
        public event Action<EquipSlotType> OnUnequipped;
        public event Action OnClearAll;
        public event Action<ItemConfig> OnItemBroken;

        public event Action OnUpdate
        {
            add { }
            remove { }
        }

        public void RestoreEquipmentFromInventory()
        {
            
        }

        public IReadOnlyList<ItemStatEntry> GetEquippedModifiers()
        {
            return null;
        }

        public EquipmentSnapshot CreateReadonlySnapshot()
        {
            return null;
        }

        public void ApplyDurabilityDamage(float damage)
        {
            
        }
    }
}
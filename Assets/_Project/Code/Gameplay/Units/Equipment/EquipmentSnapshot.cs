using System.Collections.Generic;
using Galactic1.Items;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Equipment.Snapshots
{
    /// <summary>
    /// Readonly-снимок экипировки юнита.
    /// Используется в рейде / симуляциях.
    /// </summary>
    public sealed class EquipmentSnapshot
    {
        public readonly IReadOnlyDictionary<EquipSlotType, EquipmentItemSnapshot> Items;

        public EquipmentSnapshot(
            IReadOnlyDictionary<EquipSlotType, EquipmentItemSnapshot> items)
        {
            Items = items;
        }
    }

    public readonly struct EquipmentItemSnapshot
    {
        public readonly ItemConfig Item;
        public readonly int Durability;
        private readonly int AmmoInClip;


        public EquipmentItemSnapshot(ItemConfig item, int durability, int ammo)
        {
            Item = item;
            Durability = durability;
            AmmoInClip = ammo;
        }
    }
}
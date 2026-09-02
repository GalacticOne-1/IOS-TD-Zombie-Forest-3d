using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Stats;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Game.Meta.Items
{
    public abstract class EquipmentModuleBase : ItemModule, IEquipModule
    {
        [SerializeField] protected ItemEquipSettings settings;
        
        [Header("Equip Restrictions")]
        [SerializeField] protected EquipRestrictionData equipRestrictions;
        
        [Header("Base Stats")]
        [SerializeField] protected List<ItemStatEntry> baseStats; // "что предмет даёт юниту"


        
        
        
        public EquipSlotType GetSlot() => settings.slotType;
        public ItemEquipSettings Settings => settings;
        
        /// <summary>
        /// Передача списка стат влияющих на юнит
        /// <br/>Эти статы могут отображатся в инвентаре под юнитом
        /// </summary>
        public abstract IReadOnlyList<ItemStatEntry> BaseStats();



        // public bool CanEquip(UnitStatsRuntime unit)
        // {
        //     var restrictions = ItemConfig.EquipRestrictions;
        //
        //     if (restrictions.allowedSlot != GetSlot())
        //         return false;
        //
        //     if (restrictions.allowedClass != ItemEquipClass.None &&
        //         restrictions.allowedClass != Settings.equipClass)
        //         return false;
        //
        //     if (unit.Level < restrictions.requiredLevel)
        //         return false;
        //
        //     if (restrictions.allowedUnits != null && restrictions.allowedUnits.Count > 0)
        //         if (!restrictions.allowedUnits.Contains(unit.UnitClass))
        //             return false;
        //
        //     return true;
        // }
    }
    
    [System.Serializable]
    public struct ItemEquipSettings
    {
        public EquipSlotType slotType;
        public ItemEquipType equipType;
    }
    
    [System.Serializable]
    public struct EquipRestrictionData
    {
        public int requiredLevel;              // Минимальный уровень персонажа/юнита
        public List<UnitClass> allowedUnits;   // Список юнитов/классов, кто может использовать
    }
    
    
}
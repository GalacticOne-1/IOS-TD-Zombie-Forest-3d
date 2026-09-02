using System;
using System.Collections.Generic;
using Galactic1.Code.Items;
using Galactic1.Code.UI.Garage;
using Galactic1.Game.Meta.Stats;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Equipment used by vehicles (armor plates, engines, turrets, etc)
    /// </summary>
    [Serializable]
    public class VehicleEquipmentModule : VehicleModuleBase
    {
        [Header("Vehicle Slot")] [SerializeField]
        private VehicleEquipSettings settings;


        [Header("Vehicle Stats")]
        [SerializeField] private List<ItemStatEntry> baseStats = new();

        [Header("Vehicle Modifiers")]
        [SerializeField] private List<StatModifier> modifiers = new();


        
        
        public VehicleEquipSettings Settings => settings;
        public IReadOnlyList<ItemStatEntry> BaseStats => baseStats;
        public IReadOnlyList<StatModifier> Modifiers => modifiers;
        
        
        
        
        

        public IReadOnlyList<ItemStatEntry> GetStats() => baseStats;
        
        public float GetStat(StatId statId)
        {
            foreach (var s in baseStats)
                if (s.StatId == statId)
                    return s.Value;

            return 0;
        }
        public override void CollectDescriptors(List<DescriptorDisplayEntry> list)
        {
            
        }
    }
    
    [System.Serializable]
    public struct VehicleEquipSettings
    {
        public VehicleSlotType slotType;
        public ItemEquipType equipType;
    }
}
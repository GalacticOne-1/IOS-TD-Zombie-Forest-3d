using System.Collections.Generic;
using Galactic1.Code.Items;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Stats;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Транспорт для карты
    /// - инвентарь
    /// - места для отряда
    /// </summary>
    [System.Serializable]
    public class VehicleModule : VehicleModuleBase
    {
        [SerializeField] private int cargoCapacity = 15;

        [SerializeField] private int squadSlots = 4;
        
        
        public int CargoCapacity => cargoCapacity;
        public int SquadSlots => squadSlots;
        
        
        
        
        
        public  IReadOnlyList<ItemStatEntry> BaseStats()
        {
            var ar = new List<ItemStatEntry>();
            ar.Add(new ItemStatEntry()
            {
                StatId = StatId.MaxSquadCapacity,
                Operation = ModifierOperation.Flat,
                Value = squadSlots,
                applyToUnit = false,
                showInTooltip = true,
            });
            ar.Add(new ItemStatEntry()
            {
                StatId = StatId.InventoryCapacity,
                Operation = ModifierOperation.Flat,
                Value = cargoCapacity,
                applyToUnit = false,
                showInTooltip = true,
            });
            
            return ar;
        }
    }
}
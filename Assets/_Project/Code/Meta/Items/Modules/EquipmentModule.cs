using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Items;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Stats;
using Galactic1.UI;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Модуль экипировки (броня, одежда и т.п.)
    /// </summary>
    [System.Serializable]
    public class EquipmentModule : EquipmentModuleBase
    {

        public override IReadOnlyList<ItemStatEntry> BaseStats()
        {
            var ar = new List<ItemStatEntry>(baseStats.ToList());
            
            ar.Add(new ItemStatEntry()
            {
                StatId = StatId.Durability,
                Operation = ModifierOperation.Flat,
                Value = item.Physical.maxDurability,
                applyToUnit = false,
                showInTooltip = true,
            });

            return ar;
        }
        

        
        public override void CollectDescriptors(List<DescriptorDisplayEntry> list)
        {
            list.Add(new(DescriptorId.ArmorType, 
                settings.slotType, ValueType.Enum));
        }
        
        public override void BuildTooltip(ref TooltipItemDto data)
        {
            data.stats = BaseStats();

            data.descriptors = new[]
            {
                new DescriptorDisplayEntry()
                {
                    DescriptorId = DescriptorId.ArmorType,
                    RawValue = settings.slotType,
                    ValueType = ValueType.Enum
                }
            };
        }
        
        public override CompareStat StatCompare(StatId toCompare, float value)
        {
            var stats = BaseStats();
            var l = stats.Count;
            for (int i = 0; i < l; i++)
            {
                if (stats[i].StatId == toCompare)
                    return value > stats[i].Value ? CompareStat.More : CompareStat.Less;
            }

            return CompareStat.Fail;
        }
    }

    
}
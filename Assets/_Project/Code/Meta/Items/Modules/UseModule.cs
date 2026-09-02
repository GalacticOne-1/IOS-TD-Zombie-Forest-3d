
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Items;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Stats;
using Galactic1.UI;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Поведение при использовании предмета (расходники, еда, вода и т.п.)
    /// </summary>
    [System.Serializable]
    public class UseModule : ItemModule, IEquipModule
    {
        [SerializeField] private bool consumeOnUse = true;
        [Header("!!!! Legacy !!!!!")]
        [SerializeField] private List<StatModifier> effects;
        [SerializeField] private EquipSlotType slotType = EquipSlotType.Usable;

        [Space]
        [SerializeReference] private ConsumableBehaviour behaviour;
        [SerializeField] private float range = 10f; // макс дистанция для броска
        
        
        public bool ConsumeOnUse => consumeOnUse;
        public IReadOnlyList<StatModifier> Effects => effects;
        public EquipSlotType GetSlot() => slotType;
        public ConsumableBehaviour Behaviour => behaviour;

        public float Range => range;


        public bool SetConsume { set => consumeOnUse = value; }
        public void SetEffects(List<StatModifier> list)
        {
            effects = new List<StatModifier>(list);
        }
        
        
        
        
        
        public IReadOnlyList<ItemStatEntry> BaseStats()
        {
            var ar = new List<ItemStatEntry>();
            if (behaviour is HealBehaviour medical)
            {
                ar.Add(new ItemStatEntry()
                    {
                        StatId = StatId.RestoreHealth,
                        Operation = ModifierOperation.Flat,
                        Value = (int)medical.healAmount,
                        applyToUnit = false,
                        showInTooltip = true,
                    }
                );
            }
            
            else if(behaviour is GrenadeBehaviour grenade)
            {
                if (grenade.Damage > 0)
                {
                    ar.Add(new ItemStatEntry()
                    {
                        StatId = StatId.AoeDamage,
                        Operation = ModifierOperation.Flat,
                        Value = (int)grenade.Damage,
                        applyToUnit = false,
                        showInTooltip = true,
                    });
                }

                if (grenade.ZoneConfig)
                {
                    if (grenade.ZoneConfig.damagePerTick > 0)
                        ar.Add(new ItemStatEntry()
                        {
                            StatId = StatId.AoeDamage,
                            Operation = ModifierOperation.Flat,
                            Value = (int)grenade.ZoneConfig.damagePerTick,
                            applyToUnit = false,
                            showInTooltip = true,
                        });
                    
                    if(grenade.ZoneConfig.stunDuration > 0)
                        ar.Add(new ItemStatEntry()
                        {
                            StatId = StatId.Duration,
                            Operation = ModifierOperation.Flat,
                            Value = (int)grenade.ZoneConfig.stunDuration,
                            applyToUnit = false,
                            showInTooltip = true,
                        });
                }
                
                ar.Add(new ItemStatEntry()
                {
                    StatId = StatId.AoeRange,
                    Operation = ModifierOperation.Flat,
                    Value = (int)grenade.OuterExplosionRadius,
                    applyToUnit = false,
                    showInTooltip = true,
                });
            }

            return ar;
        }

        public override void CollectDescriptors(List<DescriptorDisplayEntry> list)
        {
            
        }
        
        
        public override void BuildTooltip(ref TooltipItemDto data)
        {
            data.stats = BaseStats();
            
            if(behaviour is HealBehaviour medical)
            {
                data.descriptors = new[]
                {
                    new DescriptorDisplayEntry()
                    {
                        DescriptorId = DescriptorId.Medical,
                        RawValue = "Medical Consumables",
                        ValueType = ValueType.String
                    }
                };
            }
            
            else if(behaviour is GrenadeBehaviour grenade)
            {
                data.descriptors = new[]
                {
                    new DescriptorDisplayEntry()
                    {
                        DescriptorId = DescriptorId.Grenade,
                        RawValue = "Grenade",
                        ValueType = ValueType.String
                    }
                };
            }
        }
    }
}
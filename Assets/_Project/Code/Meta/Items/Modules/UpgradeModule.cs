using System.Collections.Generic;
using Galactic1.Code.Items;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Модуль улучшения (обвесы, апгрейды)
    /// </summary>
    [System.Serializable]
    public class UpgradeModule : ItemModule
    {
        [SerializeField] private UpgradeSlotType slotType;
        
        // Совместимость — к каким предметам подходит
        [SerializeField] private List<ItemTag> compatibleTags = new();
        [SerializeField] private List<StatModifier> modifiers;


        public UpgradeSlotType SlotType => slotType;
        public List<ItemTag> CompatibleTags => compatibleTags;
        public List<StatModifier> Modifiers => modifiers;

        
        
        public override void CollectDescriptors(List<DescriptorDisplayEntry> list)
        {
            
        }
    }
    
    public enum UpgradeSlotType
    {
        Scope,
        Suppressor,
        Magazine,
        Grip,
        Stock,
        Barrel
    }
}
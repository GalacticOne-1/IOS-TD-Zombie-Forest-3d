using System;
using System.Collections.Generic;
using Galactic1.Code.Items;
using Galactic1.UI;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Base class for all Item modules.
    /// Stores reference to parent ItemConfig.
    /// </summary>
    [Serializable]
    public abstract class ItemModule : 
        IItemModule, 
        IDescriptorProvider, 
        ITooltipProvider
    {
        [NonSerialized]
        protected ItemConfig item;

        public ItemConfig Item => item;

        public virtual void OnItemCreated(ItemConfig item)
        {
            this.item = item;
        }
        
        
        public virtual void CollectDescriptors(List<DescriptorDisplayEntry> list){}

        public virtual void BuildTooltip(ref TooltipItemDto data) {}

        /// <summary>
        /// Сравнение одной статы
        /// </summary>
        /// <param name="toCompare"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual CompareStat StatCompare(StatId toCompare, float value) => CompareStat.Fail;
    }
    
    public interface IItemModule
    {
        void OnItemCreated(ItemConfig item);
    }

    public enum CompareStat
    {
        Fail,
        Less,
        More
    }
}
using System;
using Galactic1.Core.Enums;
using Galactic1.Game.Runtime.Production;
using UnityEngine;

namespace Galactic1.Game.UI.Production.DTO
{
    /// <summary>
    /// DTO производственного задания.
    /// </summary>
    public sealed class ProductionJobDTO
    {
        public string JobId { get; }
        public Sprite Icon { get; }
        
        public ProductionJobState State { get; }
        public int TotalHours { get; }
        public int RemainingHours { get; }
        
        public ItemRarity Rarity { get; }
        public int Amount { get; }
        public int TotalStack { get; }
        public int CompletedStack { get; }
        

        public ProductionJobDTO(
            string jobId,
            Sprite icon,
            ItemRarity rarity,
            int totalHours,
            int remainingHours,
            int amount,
            int totalStack,
            int completedStack,
            ProductionJobState state)
        {
            JobId = jobId;
            Icon = icon;
            Rarity = rarity;
            TotalHours = totalHours;
            RemainingHours = remainingHours;
            Amount = amount;
            TotalStack = totalStack;
            CompletedStack = completedStack;
            State = state;
            
        }
    }
}
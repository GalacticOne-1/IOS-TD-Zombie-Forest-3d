
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Code.UI.Stations
{
    public readonly struct SlotStatusDTO
    {
        public readonly int Index;
        public readonly bool IsActive;
        public readonly bool IsCompleted;
        public readonly float Progress;
        public readonly int RemainingHours; // int как в ProductionJobDTO
        public readonly int TotalHours; // для расчёта fillAmount
        public readonly Sprite ItemIcon;
        public readonly ItemRarity Rarity;
        public readonly int CompletedCount;
        public readonly int TotalCount;

        public SlotStatusDTO(
            int index,
            bool isActive,
            bool isCompleted,
            float progress,
            int remainingHours,
            int totalHours,
            Sprite itemIcon,
            ItemRarity rarity,
            int completedCount,
            int totalCount)
        {
            Index = index;
            IsActive = isActive;
            IsCompleted = isCompleted;
            Progress = progress;
            RemainingHours = remainingHours;
            TotalHours = totalHours;
            ItemIcon = itemIcon;
            Rarity = rarity;
            CompletedCount = completedCount;
            TotalCount = totalCount;
        }

        public static SlotStatusDTO Empty(int index)
            => new(index, false, false, 0f, 0, 0, null, ItemRarity.Common, 0, 0);
    }
}
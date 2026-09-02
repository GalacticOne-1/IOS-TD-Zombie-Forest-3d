
using UnityEngine;

namespace Galactic1.Runtime.UI.WorldStatus
{
    /// <summary>
    /// Снимок состояния верстака для World Status UI.
    /// Не хранит ссылок на runtime — только plain-данные.
    /// </summary>
    public readonly struct WorkbenchStatusDTO
    {
        /// <summary>Иконка производимого предмета (первый слот в очереди).</summary>
        public readonly Sprite ItemIcon;

        /// <summary>Количество готовых заказов.</summary>
        public readonly int CompletedStack;

        /// <summary>Всего заказов в активном слоте (completed + в работе).</summary>
        public readonly int TotalStack;

        public readonly int RemainingTime;
        /// <summary>Прогресс текущего заказа [0..1]. 0 = начало, 1 = готов.</summary>
        public readonly float Progress;

        /// <summary>Есть ли активная работа (InProgress).</summary>
        public readonly bool IsWorking;

        /// <summary>Есть ли хоть что-то в очереди.</summary>
        public readonly bool HasAnyJob;

        public WorkbenchStatusDTO(
            Sprite itemIcon,
            int    completedStack,
            int    totalStack,
            int remainingTime,
            float  progress,
            bool   isWorking,
            bool   hasAnyJob)
        {
            ItemIcon        = itemIcon;
            CompletedStack = completedStack;
            TotalStack     = totalStack;
            RemainingTime = remainingTime;
            Progress        = progress;
            IsWorking       = isWorking;
            HasAnyJob       = hasAnyJob;
        }

        public static WorkbenchStatusDTO Empty 
            => new(null, 0, 0, -1, 0f, false, false);
    }
}
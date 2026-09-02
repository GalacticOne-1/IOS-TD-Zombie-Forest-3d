using System;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.RaidLoot.Authoring
{
    [Serializable]
    public struct LootSlotConfig
    {
        [Tooltip("Уникальное имя слота для отладки.")]
        public string SlotId;

        [Tooltip("Переиспользуемый пул предметов для этого слота.")]
        public LootPoolConfig SharedPool;

        [Tooltip("Сколько раз мы итерируем этот слот (бросаем кубик).")]
        [Min(1)] public int RepeatCount;

        [Tooltip("Если true — этот слот ВСЕГДА генерирует предмет (игнорирует budget).")]
        public bool IsGuaranteed;
        
        [Tooltip("Вероятность что слот вообще активируется (1.0 = всегда).")] [Range(0f, 1f)]
        public float ActivationChance;

        [Tooltip("Минимальный тир предмета допустимый в этом слоте.")]
        public Tier MinTier;

        [Tooltip("Максимальный тир предмета допустимый в этом слоте.")]
        public Tier MaxTier;
    }
}
using System;
using System.Collections.Generic;
using Galactic1.Code.Items;
using Galactic1.Gameplay;
using Galactic1.RaidLoot.Authoring;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Лутовые характеристики предмета.
    ///
    /// Используется системой генерации лута:
    /// - бюджет контейнера
    /// - tier-фильтрация
    /// - нормализация
    /// - статистика симуляции
    /// </summary>
    [Serializable]
    public sealed class LootModule : ItemModule
    {
        [Header("Economy")] [SerializeField] 
        private int lootCost = 1;

        
        [SerializeField] private bool isStrategicResource;

        [SerializeField] 
        private LootDropTag dropTag = LootDropTag.Generic;

        
        
        public int LootCost => lootCost;
        public bool IsStrategicResource => isStrategicResource;
        public LootDropTag DropTag => dropTag;
        

        public override void CollectDescriptors(List<DescriptorDisplayEntry> list)
        {
        }
        
        
        
    }
}
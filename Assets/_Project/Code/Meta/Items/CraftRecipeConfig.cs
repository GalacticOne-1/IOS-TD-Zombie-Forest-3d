
using System.Collections.Generic;
using Galactic1.Code.Systems.Economy;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Stations;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Recipe that produces specific ItemConfig.
    /// Can be used by crafting stations.
    /// </summary>
    [CreateAssetMenu(fileName = "CraftRecipe", menuName = "Game Configs/Crafting/Recipe")]
    public class CraftRecipeConfig : ScriptableObject
    {
        [Header("Result")]
        [SerializeField] private ItemConfig outputItem;
        [SerializeField] private int outputCount = 1;
        
        public ItemConfig SetOutputItem { set => outputItem = value; }

        [Header("Ingredients")]
        [SerializeField] private List<RequirementData> requirement = new();

        [Header("Craft Settings")]
        [SerializeField] private ItemConfig requiredStationItem;
        [SerializeField] private Tier requiredTier = Tier.T1;
        [SerializeField] private float craftTime = 5f;
        [SerializeField] private int stackOrderLimit = 1;

        [Header("Unlock Requirements")]
        [SerializeField] private int requiredPlayerLevel;
        [SerializeField] private List<ItemTag> requiredTags;

        
        
        
        public ItemConfig OutputItem => outputItem;
        public int OutputCount => outputCount;
        public IReadOnlyList<RequirementData> Requirement => requirement;
        public ItemConfig RequiredStationItem => requiredStationItem;
        public Tier RequiredTier => requiredTier;
        public float CraftTime => craftTime;
        /// <summary>
        /// Максимальное количество заказов, которое может быть объединено в один слот.
        /// 1 = каждый заказ отдельный.
        /// </summary>
        public int StackOrderLimit => stackOrderLimit;
        
        public int RequiredPlayerLevel => requiredPlayerLevel;
        public IReadOnlyList<ItemTag> RequiredTags => requiredTags;
        
        
        
        
        public void SetRequiredStation(ItemConfig station)
        {
            requiredStationItem = station;
        }
        
        public void SetIngredients(List<RequirementData> list)
        {
            requirement = new (list);
        }
        
        
        public void AddIngredient(ItemConfig item)
        {
            if (item == null)
                return;

            if (requirement == null)
                requirement = new ();

            // защита от дублей
            foreach (var r in requirement)
            {
                if (r.Item == item)
                    return;
            }

            requirement.Add(new RequirementData
            {
                Item = item
            });
        }
        
        
        /// <summary>
        /// Проверяет может ли станция крафтить этот рецепт по уровню.
        /// Level станции — 0-based, Tier рецепта — 1-based.
        /// </summary>
        public bool CanCraftAtLevel(int stationLevel)
            => stationLevel + 1 >= (int)RequiredTier;
    }

}
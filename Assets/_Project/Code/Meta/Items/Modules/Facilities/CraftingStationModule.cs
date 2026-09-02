using System.Collections.Generic;
using Galactic1.Game.Meta.Stations;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Если предмет является верстаком.
    /// </summary>
    [System.Serializable]
    public class CraftingStationModule : FacilityModule
    {

        #region Available Recipes

        private List<ItemConfig> availableRecipes;

        public IReadOnlyList<ItemConfig> AvailableRecipes => availableRecipes;

        public void ClearAvailableRecipes()
            => availableRecipes = new();

        public void AddToRecipeList(ItemConfig item)
            => availableRecipes.Add(item);



        public void SortRecipes()
        {
            if (availableRecipes == null) return;

            // Сортируем по минимальному тиру среди рецептов предмета
            // Предмет может иметь несколько рецептов — берём минимальный тир
            availableRecipes.Sort((a, b) =>
            {
                int tierA = GetMinRequiredTier(a);
                int tierB = GetMinRequiredTier(b);
                return tierA.CompareTo(tierB);
            });
        }

        private static int GetMinRequiredTier(ItemConfig item)
        {
            int min = int.MaxValue;

            foreach (var recipe in item.Recipes)
            {
                int tier = (int)recipe.RequiredTier;
                if (tier < min)
                    min = tier;
            }

            // Если рецептов нет — в конец списка
            return min == int.MaxValue ? 999 : min;
        }

        #endregion



    }

}
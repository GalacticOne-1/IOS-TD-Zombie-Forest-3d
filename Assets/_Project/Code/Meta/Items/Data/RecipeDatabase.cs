using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Configs;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;

namespace Galactic1.Crafting
{
    
    /*
     *  База данных, которая хранит все рецепты и позволяет искать их по предмету, станции и т.д.
     */
    
    [CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Game Configs/Inventory/New Recipe Database")]
    public class RecipeDatabase : ScriptableObject
    {
        public List<CraftRecipeConfig> recipes = new ();

        public CraftRecipeConfig GetRecipeByOutput(ItemConfig item)
            => recipes.FirstOrDefault(r => r.OutputItem == item);

        
    }

}
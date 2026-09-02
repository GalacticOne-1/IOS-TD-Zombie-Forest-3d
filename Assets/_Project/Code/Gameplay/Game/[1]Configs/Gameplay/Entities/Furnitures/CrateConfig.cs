using UnityEngine;

namespace Galactic1
{
    [CreateAssetMenu(fileName = "CrateConfig", menuName = "Game Configs/Inventory/New Crate Config")]
    public class CrateConfig : BuildableConfig
    {
        
        [Space] 
        [SerializeField] 
        [Header("Требования для улучшения")]
        private CCraft recipeUpgrade;

        public CCraft RecipeUpgrade => recipeUpgrade;


    }
}
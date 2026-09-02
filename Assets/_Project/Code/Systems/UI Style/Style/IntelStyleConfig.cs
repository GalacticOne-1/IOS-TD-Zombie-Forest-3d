using System;
using System.Linq;
using Galactic1.Code.WorldMap.Intel;
using Galactic1.RaidLoot.Authoring;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.UI.Core
{
    [CreateAssetMenu(
        fileName = "IntelStyleConfig", 
        menuName = "Game Configs/Style/Intel Style Config")]
    public class IntelStyleConfig : StyleConfigBase
    {
        
        [field: SerializeField] public Sprite unknownIcon { get; private set; }
        [field: SerializeField] public Sprite noneIcon { get; private set; }

        [Header("Threat Level")]
        [field: SerializeField]
        public IntelIconSet threatLevel { get; private set; }

        [Header("Enemy Type")]
        [field: SerializeField]
        public IntelIconSet enemyType { get; private set; }

        [Header("Hazard Type")]
        [field: SerializeField]
        public IntelIconSet hazardType { get; private set; }
        
        [Header("Operational Risk")]
        [field: SerializeField]
        public IntelIconSet riskLevel { get; private set; }


        
        [Header("Loot Profile")]
        [SerializeField] private ResourceVolumes[] resourcesVolume;
        [SerializeField] private IntelIconSet[] lootCategories;





        /// <summary>
        /// Вернет цвет для иконки объема ресурса
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public ResourceVolumes GetColorCategoryVolume(ResourceVolume category)
        {
            var l = resourcesVolume.Length;
            for (int i = 0; i < l; i++)
            {
                if (resourcesVolume[i].type == category)
                    return resourcesVolume[i];
            }
            return resourcesVolume[0];
        }

        /// <summary>
        /// Вернет иконку ресурса по категории
        /// </summary>
        /// <param name="category"></param>
        /// <param name="set"></param>
        public void GetIconSet(LootEconomyCategory category, out IntelIconSet set)
        {
            var l = lootCategories.Length;
            for (int i = 0; i < l; i++)
            {
                if (lootCategories[i].category.Contains(category))
                {
                    set = lootCategories[i];
                    return;
                }
            }

            set = null;
        }
    }


    [Serializable]
    public class IntelIconSet
    {
        public Sprite activeIcon;
        
        [Tooltip("For loot group")]
        public LootEconomyCategory[] category;
    }

    [Serializable]
    public class ResourceVolumes
    {
        public ResourceVolume type;
        public Color color;
        public Sprite sprite;
    }
}
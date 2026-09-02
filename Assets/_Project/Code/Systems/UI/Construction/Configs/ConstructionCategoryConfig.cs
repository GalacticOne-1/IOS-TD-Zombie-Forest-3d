using System;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Systems.Construction.Configs
{
    /// <summary>
    /// Конфиг категории строительства.
    /// Определяет вкладку и какие здания в неё входят.
    /// </summary>
    [Serializable]
    public class ConstructionCategoryConfig
    {
        public ConstructionCategory Category;
        public string Title;
        public Sprite Icon;

        /// <summary>
        /// Типы зданий входящие в категорию
        /// </summary>
        public FacilityType[] FacilityTypes;



        public bool MatchCategory(FacilityModule facility)
        {
            // === разделяем станции оружия и обычной переработки
            foreach (var type in FacilityTypes)
            {
                if (Category == ConstructionCategory.Refinery 
                    && facility.FacilityType == type && !facility.WeaponProduction)
                    return true;
                
                if (Category == ConstructionCategory.Weapon 
                    && facility.FacilityType == type && facility.WeaponProduction)
                    return true;
            }
            
            // === для всего остального
            if (Category != ConstructionCategory.Refinery && Category != ConstructionCategory.Weapon)
                foreach (var type in FacilityTypes)
                {
                    if (facility.FacilityType == type)
                        return true;
                }

            return false;
        }
    }
}
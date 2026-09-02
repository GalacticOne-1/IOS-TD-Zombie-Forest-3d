using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Economy;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Game.Meta
{
    /// <summary>
    /// Данные одного уровня апгрейда здания.
    /// Используется всеми типами FacilityModule.
    /// </summary>
    [Serializable]
    public class FacilityUpgradeConfig
    {
        [Header("Tier")]
        [SerializeField] private Tier tier;

        [Header("Upgrade Cost")]
        [SerializeField] private List<RequirementData> requirements = new();

        public Tier Tier
        {
            get => tier;
            set => tier = value;
        }
        public IReadOnlyList<RequirementData> Requirements => requirements;
        
        
        public void AddRequirement(ItemConfig item)
        {
            if (requirements == null)
                requirements = new List<RequirementData>();

            requirements.Add(new RequirementData { Item = item, Amount = 1 });
        }

        public void SetRequirements(List<RequirementData> list)
        {
            requirements = new List<RequirementData>(list);
        }
    }
}
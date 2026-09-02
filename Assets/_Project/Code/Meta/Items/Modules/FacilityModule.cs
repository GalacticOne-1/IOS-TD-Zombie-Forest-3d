using System;
using System.Collections.Generic;
using Galactic1.Code.Items;
using Galactic1.Code.Gameplay.Grid;
using Galactic1.Code.Systems.Runtime.Building;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    [Serializable]
    public abstract class FacilityModule : ItemModule, IFacilityModule
    {
        [SerializeField] private FacilityType facilityType;
        [SerializeField] private bool weaponProduction;
        [SerializeField] private FacilityFootprintConfig footprint;

        [Header("Build Limit")] [SerializeField]
        private int buildLimit = 1; // 0 = без лимита

        [Header("Upgrades")]
        [Tooltip("Уровень 1 = базовое здание, уровень 2+ = апгрейды. Пусто = здание не апгрейдится.")]
        [SerializeField]
        private List<FacilityUpgradeConfig> upgradeLevels = new();

        [Header("Zone Restrictions")]
        [Tooltip("Теги статических зон, в которых разрешено строить это здание несмотря на блокировку")]
        [SerializeField]
        private List<GridZoneTag> allowedZoneTags = new();
        
        [Tooltip("Если включено — здание можно строить ТОЛЬКО в зонах из allowedZoneTags. " +
                 "None (незаблокированная земля) больше не разрешена автоматически, " +
                 "если явно не добавлена в список.")]
        [SerializeField]
        private bool allowedZonesOnly = false;


        public FacilityModule FacilityConfig => this;
        public FacilityType FacilityType => facilityType;

        public bool WeaponProduction => weaponProduction;

        public FacilityFootprintConfig FootprintConfig => footprint;

        public int BuildLimit => buildLimit;

        public bool IsUpgradeable => upgradeLevels.Count > 0;

        public int MaxLevel => upgradeLevels.Count;

        /// <summary>
        /// Разрешено ли строить это здание в зоне с данным тегом.
        /// GridZoneTag.None означает "клетка не заблокирована" — всегда разрешено.
        /// </summary>
        public bool IsZoneAllowed(GridZoneTag tag)
        {
            if (allowedZonesOnly)
                return allowedZoneTags.Contains(tag);
            
            if (tag == GridZoneTag.None)
                return true;

            return allowedZoneTags.Contains(tag);
        }


#if UNITY_EDITOR
        public List<FacilityUpgradeConfig> GetUpgradeLevelsForEditor() => upgradeLevels;
#endif


        public FacilityUpgradeConfig GetUpgrade(int toLevel)
        {
            var index = toLevel - 1;
            if (index < 0 || index >= upgradeLevels.Count)
                return null;

            return upgradeLevels[index];
        }

        public bool IsLimitReached(int builtCount)
            => buildLimit > 0 && builtCount >= buildLimit;


        public override void CollectDescriptors(List<DescriptorDisplayEntry> list)
        {

        }
    }

    public interface IFacilityModule : IItemModule
    {
        FacilityModule FacilityConfig { get; }
        FacilityType FacilityType { get; }
        FacilityFootprintConfig FootprintConfig { get; }
    }
}
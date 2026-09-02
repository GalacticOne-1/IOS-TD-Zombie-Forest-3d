using System.Collections.Generic;
using Galactic1.Code.Systems.Runtime.Building;
using UnityEngine;

namespace Galactic1.Game.UI.Buildings.DTO
{
    /// <summary>
    /// Универсальный DTO здания лагеря.
    /// Используется UI панелью здания.
    /// </summary>
    public sealed class FacilityDTO
    {
        public string Id { get; }
        public string ConfigId { get; }

        public string DisplayName { get; }
        public string Description { get; }
        public Sprite StationIcon { get; }
        
        public int Level { get; }
        public int MaxLevel{ get; }

        public bool CanUpgrade{ get; }

        //public List<ItemStackDTO> UpgradeCost;

        public List<string> DependencyWarnings;

        public FacilityStatusDTO Status;

        public IFacilityDetailsDTO Details;


        public FacilityDTO(
            string id, 
            string configId, 
            string displayName, 
            string description, 
            Sprite stationIcon, 
            int level, 
            int maxLevel, 
            bool canUpgrade,
            List<string> dependencyWarnings, 
            FacilityStatusDTO status, 
            IFacilityDetailsDTO details)
        {
            
            Id = id;
            ConfigId = configId;
            DisplayName = displayName;
            Description = description;
            StationIcon = stationIcon;
            Level = level;
            MaxLevel = maxLevel;
            CanUpgrade = canUpgrade;
            
            DependencyWarnings = dependencyWarnings;
            Status = status;
            Details = details;
            
        }
    }
    
    /// <summary>
    /// Общий статус здания.
    /// </summary>
    public sealed class FacilityStatusDTO
    {
        public bool IsActive;
        public bool IsUnderConstruction;
        public float ConstructionProgress;
    }
    
    public interface IFacilityDetailsDTO
    {
        FacilityType Type { get; }
    }
}
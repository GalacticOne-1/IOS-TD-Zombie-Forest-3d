using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.UI.Stations
{
    public readonly struct StationCardDTO
    {
        public readonly RuntimeId StationId;
        public readonly string Name;
        public readonly Sprite Icon;
        public readonly int Level;
        public readonly bool IsBuilt;
        public readonly int TotalRemainingTime;
        public readonly SlotStatusDTO[] Slots;
        public readonly StorageAlertDTO StorageAlert;

        public StationCardDTO(
            RuntimeId stationId, 
            string name, 
            Sprite icon,
            int level, 
            bool isBuilt,
            int totalRemainingTime,
            SlotStatusDTO[] slots, 
            StorageAlertDTO storageAlert)
        {
            StationId = stationId;
            Name = name;
            Icon = icon;
            Level = level;
            IsBuilt = isBuilt;
            TotalRemainingTime = totalRemainingTime;
            Slots = slots;
            StorageAlert = storageAlert;
        }
    }
}
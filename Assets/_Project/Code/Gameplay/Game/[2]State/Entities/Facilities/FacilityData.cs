
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Game.Runtime.Production;
using Galactic1.Utility;

namespace Galactic1.Game.Buildings.Proxy
{
    [System.Serializable]
    public class FacilityData : EntityData
    {
        
        public bool IsDead { get; set; }
        public List<KeyValuePairSerializable<StatId, float>> Stats { get; set; }
        
        // --- Позиция ---
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public int Rotation { get; set; }
        
        
        // --- Tavern ---
        public List<RecruitOfferData> TavernOffers = new();
        public int NextRefreshDay { get; set; }
        
        // --- Garage ---
        public List<string> UnlockedModules = new();
        
        // --- Производство ---
        public bool IsWorking;
        public List<ProductionJobData> ProductionQueue = new();
        public int ActiveIndex = -1;
    }
}
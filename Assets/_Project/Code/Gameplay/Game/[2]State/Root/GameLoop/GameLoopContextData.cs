
using System.Collections.Generic;
using Galactic1.Code.Systems.World.Threats;
using Galactic1.Game.Camp.Proxy;
using Galactic1.Structs;

namespace Galactic1.Code.Core
{
    [System.Serializable]
    public class GameLoopContextData
    {

        public int CurrentState { get; set; }
        
        public bool PlayerOnMap { get; set; }
        public int CurrentLocationStateId { get; set; }
        public string CurrentLocationNode { get; set; }
        
        public int CurrentDay { get; set; }
        public int RemainingHour { get; set; }
        public ThreatSaveData ThreatData { get; set; }

        public RaidResultData LastRaidResult { get; set; }

        public bool HasPendingRaidReport { get; set; }
        public bool HasPendingBaseReport { get; set; }
        
        
        
        public List<PlayerData> PlayerUnitData { get; set; }
        public TransportData PlayerTransport { get; set; }
        public BaseData BaseData { get; set; }
        
        public List<string> SquadUnitId { get; set; }
        
        
        public int RemainingDroneCharge { get; set; }
    }
}
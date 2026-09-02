using System;
using UnityEngine.Serialization;

namespace Galactic1.Code.Systems.World.Threats
{
    [Serializable]
    public class ThreatSaveData
    {
        public string Id;
        public int Type;
        public int Stage;

        public int CreatedAtDay;
        public int RevealDay;
        public int AttackDay;
    }
}
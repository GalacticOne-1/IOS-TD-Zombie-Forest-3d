using System;
using UnityEngine;

namespace Galactic1.Code.Game.Rewards
{
    [Serializable]
    public class RewardEntry
    {
        public RewardType type;
        public string id;      // itemId, currencyId, boosterId...
        
        [Space]
        public int amount;
        
        [Range(1,5)]
        public float multiplier = 1f; // 1 = без бонуса, >1 = бонус
    }
}
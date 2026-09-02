using System;

namespace Galactic1.Core
{
    [Serializable]
    public struct CGameStateDailyReward
    {
        public bool dailyBonusReward;
        public bool dailyBonusShowed;
        public byte dailyBonusDay;
        public CSaveDailyReward[] dailyBonus;
    }
    
    [Serializable]
    public struct CSaveDailyReward
    {
        public byte state;
    }
}
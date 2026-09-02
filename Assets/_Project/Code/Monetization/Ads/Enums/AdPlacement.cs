namespace Galactic1.Code.Core.Ads
{
    /// <summary>
    /// Точки показа рекламы в игре (placement-based модель).
    /// Это доменный уровень — не зависит от SDK.
    /// </summary>
    public enum AdPlacement
    {
        GameShop1 = 0,
        GameShop2 = 1,
        
        
        DoubleReward = 10,
        PostLevelInterstitial = 11,
        
        
        PostRaid = 20,
        Revive = 21,
        RecruitmentTavern = 22,
        RaidReportDrone,
    }
}
using System.Collections.Generic;
using Galactic1.Code.Core.Ads;

namespace Galactic1.Code.Game.Rewards
{
    /// <summary>
    /// Поставщик наград за рекламу по placement.
    /// </summary>
    public interface IAdRewardProvider : IGameService
    {
        float GetAdMultiplier(AdPlacement placement);
        IReadOnlyList<RewardEntry> GetRewards(AdPlacement placement);
    }
}
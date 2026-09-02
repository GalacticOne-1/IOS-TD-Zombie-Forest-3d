using UnityEngine;

namespace Galactic1.Code.Game.Rewards.Modifiers
{
    /// <summary>
    /// Умножает награду, если источник — реклама.
    /// </summary>
    public class AdRewardModifier : IRewardModifier
    {
        public int Order => 100;

        public int Modify(RewardEntry reward, int amount, RewardContext ctx)
        {
            if (ctx.Source != RewardSource.Ad)
                return amount;

            return Mathf.CeilToInt(amount * reward.multiplier);
        }
    }
}
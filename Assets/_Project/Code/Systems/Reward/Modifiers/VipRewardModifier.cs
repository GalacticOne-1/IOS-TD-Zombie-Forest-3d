using UnityEngine;

namespace Galactic1.Code.Game.Rewards.Modifiers
{
    public class VipRewardModifier : IRewardModifier
    {
        public int Order => 200;

        public int Modify(RewardEntry reward, int amount, RewardContext ctx)
        {
            if (!ctx.IsVip) return amount;
            return Mathf.CeilToInt(amount * 1.2f);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Game.Rewards.Modifiers;

namespace Galactic1.Code.Game.Rewards
{
    /// <summary>
    /// Последовательно применяет модификаторы к награде.
    /// </summary>
    public class RewardPipeline
    {
        private readonly List<IRewardModifier> modifiers;

        public RewardPipeline(IEnumerable<IRewardModifier> modifiers)
        {
            this.modifiers = modifiers.OrderBy(m => m.Order).ToList();
        }

        public int ResolveAmount(RewardEntry reward, RewardContext ctx)
        {
            int amount = reward.amount;

            foreach (var mod in modifiers)
                amount = mod.Modify(reward, amount, ctx);

            return amount;
        }
    }
}
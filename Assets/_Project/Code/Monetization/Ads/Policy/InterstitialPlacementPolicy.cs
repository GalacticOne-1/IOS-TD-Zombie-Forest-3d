
using System.Collections.Generic;
using Galactic1.Code.Core.Ads;

namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Управляет всеми правилами показа Interstitial рекламы.
    /// </summary>
    public class InterstitialPlacementPolicy
    {
        private readonly List<IInterstitialPlacementRule> rules = new();

        public void AddRule(IInterstitialPlacementRule rule)
            => rules.Add(rule);

        /// <summary>
        /// Проверяет все правила для placement
        /// </summary>
        public bool CanShow(AdPlacement placement, out string reason)
        {
            reason = "";
            foreach (var rule in rules)
            {
                if (!rule.CanShow(placement, out reason))
                    return false;
            }

            return true;
        }
    }
}
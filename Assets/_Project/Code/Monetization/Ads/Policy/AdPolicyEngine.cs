
using Galactic1.Code.Core.Ads;

namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Проверяет правила показа рекламы (лимиты, кулдауны, экономика).
    /// Не знает про SDK.
    /// </summary>
    public class AdPolicyEngine
    {
        private readonly AdCooldownService cooldowns;
        private readonly AdEconomyService economy;

        public AdPolicyEngine(AdCooldownService cooldowns, AdEconomyService economy)
        {
            this.cooldowns = cooldowns;
            this.economy = economy;
        }

        public AdDecision Evaluate()
        {
            if (!economy.HasDailyQuota())
                return AdDecision.Deny("Daily limit reached");

            if (cooldowns.IsOnCooldown(out float remaining))
                return AdDecision.Deny("Cooldown", remaining);

            return AdDecision.Allow();
        }
    }
}
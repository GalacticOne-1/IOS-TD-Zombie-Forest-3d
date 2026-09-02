
using Galactic1.Core.Enums;
using Galactic1.RaidLoot.Authoring;

namespace Galactic1.RaidLoot.Services.Rules
{
    /// <summary>
    /// Единственный источник истины о допустимых тирах предметов.
    ///
    /// Источники ограничений (применяются через AND — берётся наиболее строгий):
    ///   1. LootSlotConfig.MinTier / MaxTier   — дизайнерские ограничения слота
    ///   2. DepletionStageRule.MaxTierAllowed  — ограничение по стадии истощения
    ///   3. ContainerDefinition.ContainerTier  — физический тир контейнера
    ///
    /// ContextFilter вызывает только этот класс.
    /// WeightedSelector не знает про тиры.
    /// QuantityRoller не знает про тиры.
    /// </summary>
    public static class TierLimitResolver
    {
        public readonly struct TierLimits
        {
            public readonly Tier Min;
            public readonly Tier Max;

            public TierLimits(Tier min, Tier max)
            {
                Min = min;
                Max = max;
            }

            public bool Allows(Tier tier) => tier >= Min && tier <= Max;
        }

        /// <summary>
        /// Вычисляет финальные допустимые пределы тира для одного слота.
        /// Берёт наиболее строгое ограничение из всех источников.
        /// </summary>
        public static TierLimits Resolve(
            LootSlotConfig slot,
            DepletionCurveConfig.DepletionStageRule depletionStage,
            Tier containerTier)
        {
            // Min — берём максимальный из всех минимумов (наиболее строгий)
            var effectiveMin = slot.MinTier;

            // Max — берём минимальный из всех максимумов (наиболее строгий)
            var effectiveMax = slot.MaxTier;

            // Depletion stage может понижать max
            if (depletionStage.MaxTierAllowed < effectiveMax)
                effectiveMax = depletionStage.MaxTierAllowed;

            // Container tier ограничивает max
            if (containerTier < effectiveMax)
                effectiveMax = containerTier;

            // Guard: min не может быть выше max
            if (effectiveMin > effectiveMax)
                effectiveMin = effectiveMax;

            return new TierLimits(effectiveMin, effectiveMax);
        }
    }
}
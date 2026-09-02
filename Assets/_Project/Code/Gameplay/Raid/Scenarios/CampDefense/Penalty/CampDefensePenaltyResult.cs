using System.Collections.Generic;

namespace Galactic1.Code.Systems.CampDefense.Penalty
{
    /// <summary>
    /// Immutable результат работы CampDefensePenaltyCalculator.
    /// Только данные — Applier читает Items и ничего в этом объекте не меняет.
    /// </summary>
    public sealed class CampDefensePenaltyResult
    {
        public IReadOnlyList<CampDefensePenaltyItem> Items { get; }

        public bool HasPenalty => Items.Count > 0;

        public CampDefensePenaltyResult(IReadOnlyList<CampDefensePenaltyItem> items)
        {
            Items = items ?? System.Array.Empty<CampDefensePenaltyItem>();
        }

        public static CampDefensePenaltyResult Empty { get; } =
            new CampDefensePenaltyResult(System.Array.Empty<CampDefensePenaltyItem>());
    }
}
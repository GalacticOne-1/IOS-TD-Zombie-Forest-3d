using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.Systems.CampDefense.Penalty
{
    /// <summary>
    /// Отвечает только за применение уже готового результата.
    /// Никаких вычислений — только Inventory.TrySpend(...) по каждому item.
    /// </summary>
    public sealed class CampDefensePenaltyApplier
    {
        public void Apply(IInventoryResourcesPort inventory, CampDefensePenaltyResult result)
        {
            if (inventory == null || result == null || !result.HasPenalty)
                return;

            foreach (var item in result.Items)
                inventory.TrySpend(item.ItemId, item.Amount);
        }
    }
}
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Abilities
{
    /// <summary>
    /// Отложенное выполнение ability (ждёт animation event)
    /// </summary>
    public sealed class PendingAbilityExecution
    {
        public ItemUseContext Context;
        public InventorySlotRuntime Slot;
        public ConsumableBehaviour Behaviour;
    }
}
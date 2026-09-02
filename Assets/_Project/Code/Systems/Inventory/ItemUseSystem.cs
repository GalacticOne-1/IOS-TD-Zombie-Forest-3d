using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Items
{
    public static class ItemUseSystem
    {
        /// <summary>
        /// Основной метод вызова действий предмета
        /// </summary>
        public static void UseItem(ItemContext ctx)
        {
            var item = ctx.slot.Item;
            if (item == null)
            {
                Debug.LogWarning("UseItem: item is null");
                return;
            }

            if (!item.HasModule<ActionModule>() || item.Action.Actions.Count == 0)
            {
                Debug.Log($"Item {item.name} has no actions.");
                return;
            }

            foreach (var action in item.Action.Actions)
            {
                action.Execute(ctx);
            }
        }
    }
}
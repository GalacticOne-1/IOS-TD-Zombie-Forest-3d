using Galactic1.Code.UI.Tooltips;
using Galactic1.Game.Meta.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    public abstract class ItemCardBinder : MonoBehaviour
    {

        private TooltipInputHandler inputHandler;
        private ItemConfig item;


        public void Bind(ItemConfig config)
        {
            item = config;
            GetComponent<Image>().raycastTarget = true;

            // === подсказка
            inputHandler = GetComponentInChildren<TooltipInputHandler>();
            inputHandler.RegisterOnRequest(HandleHoldStart);
            inputHandler.RegisterOnCancell(HandleHoldEnd);
        }


        private void HandleHoldStart(RectTransform anchor)
        {
            if (item != null)
            {
                ServiceLocator.Current.Get<TooltipController>().Show<ItemTooltipView>(
                    TooltipType.Loot,
                    gameObject.CMP_RectTr(),
                    item,
                    item.Physical.maxDurability);
            }
        }

        private void HandleHoldEnd()
        {
            if (item != null)
                ServiceLocator.Current.Get<TooltipController>().Hide();
        }

    }
}
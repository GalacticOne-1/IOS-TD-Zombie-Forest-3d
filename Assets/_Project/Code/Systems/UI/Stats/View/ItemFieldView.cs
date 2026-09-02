using Galactic1.Code.UI.Tooltips;
using Galactic1.Game.Meta.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Game.UI.Stats
{
    public class ItemFieldView : MonoBehaviour
    {
        [SerializeField] private Image icon;



        ItemConfig itemConfig;

        public void Bind(ItemConfig item)
        {
            itemConfig = item;
            
            icon.sprite = itemConfig.Header.icon;


            // === подсказка
            var inputHandler = GetComponent<TooltipInputHandler>();
            inputHandler.RegisterOnRequest(HandleHoldStart);
            inputHandler.RegisterOnCancell(HandleHoldEnd);
        }



        private void HandleHoldStart(RectTransform anchor)
            => ServiceLocator.Current.Get<TooltipController>().Show<ItemTooltipView>(
                TooltipType.Loot,
                gameObject.CMP_RectTr(),
                itemConfig,
                0);

        private void HandleHoldEnd()
            => ServiceLocator.Current.Get<TooltipController>().Hide();
    }
}
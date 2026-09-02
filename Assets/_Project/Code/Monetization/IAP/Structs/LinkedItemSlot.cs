using Galactic1;
using Galactic1.Code.UI.Tooltips;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Shop;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LinkedItemSlot : MonoBehaviour
{
    [Header("UI Elements")]
    public Image icon;               // картинка товара
    public PriceButton purchaseButton;    // кнопка покупки
    public TMP_Text bonusText;       // бонус первой покупки
    public TMP_Text currentCount;    // текущее кол-во (limited)
    public TMP_Text oldCountText;    // старое количество
    

    private TooltipInputHandler inputHandler;
    private ItemConfig item;
    

    public void Bind(ItemConfig config)
    {
        item = config;
        
        // === подсказка
        inputHandler = GetComponentInChildren<TooltipInputHandler>();
        inputHandler.RegisterOnRequest(HandleHoldStart);
        inputHandler.RegisterOnCancell(HandleHoldEnd);
    }
    
    
    private void HandleHoldStart(RectTransform anchor)
        => ServiceLocator.Current.Get<TooltipController>().Show<ItemTooltipView>(
            TooltipType.Loot,
            gameObject.CMP_RectTr(),
            item,
            item.Physical.maxDurability);

    private void HandleHoldEnd()
        => ServiceLocator.Current.Get<TooltipController>().Hide();
}
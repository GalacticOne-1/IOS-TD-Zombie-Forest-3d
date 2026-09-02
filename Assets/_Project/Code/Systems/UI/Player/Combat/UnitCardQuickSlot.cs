
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.UnitCard
{
    public class UnitCardQuickSlot: BaseUIButton
    {
        [SerializeField] private RectTransform rootRect;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private Image icon;


        public void Bind(Sprite sprite, int count)
        {
            icon.sprite = sprite;
            countText.text = count.ToString();
            countText.gameObject.SetActive(count > 1);
        }
        
        public void Show(bool y) => gameObject.SetActive(y);
    }
}
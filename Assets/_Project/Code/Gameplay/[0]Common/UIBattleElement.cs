
using UnityEngine;
using UnityEngine.EventSystems;



namespace Galactic1
{
    public class UIBattleElement : UIElement
    {
        
        
        
        
        public override void OnPointerEnter(PointerEventData eventData)
        {
            
            UIController.I.UI_ELEMENT = true;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            UIController.I.UI_ELEMENT = false;
        }
        
        
    }
}
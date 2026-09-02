
using UnityEngine;
using UnityEngine.EventSystems;





namespace Galactic1
{
    // нужно вешать на канвас
    public class UIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        
        
        
        
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            UIController.I.UI_ELEMENT = true;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            UIController.I.UI_ELEMENT = false;
        }
        
        
    }
}
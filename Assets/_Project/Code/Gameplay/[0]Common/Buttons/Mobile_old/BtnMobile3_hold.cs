using Galactic1;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Galactic1.Mobile
{
    public class BtnMobile3_hold : CoreBtn, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        
        // без эффекта (для панелей)
        // работает удержание
        
        


        public virtual void OnPointerClick(PointerEventData eventData)
        {
            SoundClick();
            Vibro();
            if (REQUIRED_PROGRESS > 0)
            {
                //PopUp.I.ShowPopup(PopUp.EPopup.mid, $"Requires {REQUIRED_PROGRESS} nights");
                return;
            }
            
            if (ClickBlocked()) return;
            _event?.Invoke();
            Click();
        }
        public virtual void Click(){}
        
        
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (REQUIRED_PROGRESS > 0)
            {
                //PopUp.I.ShowPopup(PopUp.EPopup.mid, $"Requires {REQUIRED_PROGRESS} nights");
                return;
            }
            
            if (ClickBlocked()) return;
            Touch_down();
        }
        
        public virtual void Touch_down(){}


        public void OnPointerUp(PointerEventData eventData)
        {
            if (REQUIRED_PROGRESS > 0)
            {
                //PopUp.I.ShowPopup(PopUp.EPopup.mid, $"Requires {REQUIRED_PROGRESS} nights");
                return;
            }
            
            if (ClickBlocked()) return;
            Touch_up();
        }
        
        public virtual void Touch_up(){}
    }

}
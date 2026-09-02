using Galactic1;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Galactic1.Mobile
{
    public class BtnMobile3 : CoreBtn, IPointerClickHandler, IPointerDownHandler
    {
        
        // без эффекта (для панелей)
        
        
        
        
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (REQUIRED_PROGRESS > 0)
            {
               // PopUp.I.ShowPopup(PopUp.EPopup.mid, $"Requires {REQUIRED_PROGRESS} nights");
                return;
            }

            if (onPointerDown != null && !ClickBlocked())
            {
                SoundClick();
                Vibro();
                onPointerDown();
            }
        }
        
        


        public virtual void OnPointerClick(PointerEventData eventData)
        {
            SoundClick();
            Vibro();
            if (REQUIRED_PROGRESS > 0)
            {
                //PopUp.I.ShowPopup(PopUp.EPopup.mid, $"Requires {REQUIRED_PROGRESS} nights");
                return;
            }
            
            if (!ClickBlocked())
            {
                _event?.Invoke();
                OnClick();
            }
        }
        public override void OnClick(){}
        
    }

}
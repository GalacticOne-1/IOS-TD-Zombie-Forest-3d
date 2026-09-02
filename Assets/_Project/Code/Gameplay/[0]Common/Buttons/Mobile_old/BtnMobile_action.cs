using Galactic1;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Galactic1.Mobile
{
    public class BtnMobile_action : CoreBtn, IPointerDownHandler, IPointerUpHandler
    {
        
        /*
         *      Кнопка с активным нажатием (down & up)
         */

        
        public DFunc onPointerUp;
        
        
        
        
        public void OnPointerDown(PointerEventData eventData)
        {
            SoundClick();
            Vibro();
            if (REQUIRED_PROGRESS > 0)
            {
                //PopUp.I.ShowPopup(PopUp.EPopup.mid, $"Requires {REQUIRED_PROGRESS} nights");
                return;
            }

            onPointerDown?.Invoke();
        }
        
        

        public void OnPointerUp(PointerEventData eventData)
        {
            SoundClick();
            Vibro();
            if (REQUIRED_PROGRESS > 0)
            {
                //PopUp.I.ShowPopup(PopUp.EPopup.mid, $"Requires {REQUIRED_PROGRESS} nights");
                return;
            }

            onPointerUp?.Invoke();
        }
    }
}
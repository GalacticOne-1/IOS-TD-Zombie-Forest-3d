using Galactic1;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Mobile
{
    public class BtnMobil3_ext : BtnMobile3
    {
        /*
         *      Дополнительно меняем остояние текста и иконки внутри кнопки
         */
        
        
        //[SerializeField] private Color colorText;
        
        protected override void State_Enable()
        {
            //gameObject.GetChild(0).GetComponent<TextMeshProUGUI>().color =
                //ServiceLocator.Current.Get<IconHub>().GetColorTxt(txtStateOn);
            
            if(gameObject.GetChild(0).transform.childCount > 0)     // icon in text
            {
                var c = Color.white;
                c.a = 1;
                gameObject.GetChild(0, 0).GetComponent<Image>().color = c;
            }
        }

        protected override void State_Disable()
        {
            // var c = ServiceLocator.Current.Get<IconHub>().GetColorTxt(txtStateOff);
            // c.a = .3f;
            // gameObject.GetChild(0).GetComponent<TextMeshProUGUI>().color = 
            //     ServiceLocator.Current.Get<IconHub>().GetColorTxt(txtStateOff);
            // if(gameObject.GetChild(0).transform.childCount > 0)     // icon in text
            // {
            //     c = Color.white;
            //     c.a = .5f;
            //     gameObject.GetChild(0, 0).GetComponent<Image>().color = c;
            // }
        }


        protected override void State_TXT_Regular()
        {
            //gameObject.GetChild(0).GetComponent<TextMeshProUGUI>().color = ServiceLocator.Current.Get<IconHub>().GetColorTxt(txtStateOn);
        }

        protected override void State_TXT_Alert()
        {
            //gameObject.GetChild(0).GetComponent<TextMeshProUGUI>().color = ServiceLocator.Current.Get<IconHub>().SystemColor(EColor.red);
        }
    }
}
using Galactic1;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Galactic1.Mobile
{
    public class BtnMobile3_check : CoreBtn, IPointerClickHandler
    {
        
        /*
         *      без эффекта (для панелей)
         *      проверка разрешения для клика
         *      отмена подсветки
         */

        
        public GameObject sl;
        public DFuncResponse onClick;       // вызов функции с ответом

        
        /// <summary>
        /// Подписка нужных методов для корректной работы кнопки
        /// </summary>
        /// <param name="_onClick"></param>
        /// <param name="onCancel"></param>
        public void Subscription(DFuncResponse _onClick, ref DFunc onCancel)
        {
            onClick = _onClick;
            onCancel += OnCancel;
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
                if (onClick != null && onClick.Invoke())
                {
                    OnClick();
                    OnSelect();
                }
            }
        }

        
        public override void CallClick()
        {
            if (onClick != null && onClick.Invoke())
            {
                OnClick();
                OnSelect();
            }
        }
        
        
        /// <summary>
        /// Для функции при выборе кнопки
        /// </summary>
        public void OnSelect() => sl.SetActive(true);

        /// <summary>
        /// Для функции отмены кнопки
        /// </summary>
        public void OnCancel() => sl.SetActive(false);

    }

}

using System.Collections;
using Galactic1;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Galactic1.Mobile
{
    public class BtnMobile2 : CoreBtn, IPointerClickHandler
    {
        
        // изменение размера

        //[SerializeField, Header("Изменения размера")]
        private float smooth = .08f;
        private Vector2 size;
        
        
        
        

        public void OnPointerClick(PointerEventData eventData)
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
                StopAllCoroutines();
                StartCoroutine(e());
            }
        }

        IEnumerator e()
        {
            for (float i = 1; i > .85f; i-= smooth)
            {
                yield return null;
                size.x = size.y = i;
                gameObject.transform.localScale = size;
            }
            
#if  UNITY_EDITOR || UNITY_IOS
            yield return new WaitForSeconds(.02f);
#else
            yield return new WaitForSeconds(.05f);
#endif
            
            for (float i = size.x; i < 1; i+= smooth)
            {
                yield return null;
                size.x = size.y = i;
                gameObject.transform.localScale = size;
            }

            size.x = size.y = 1;
            gameObject.transform.localScale = size;
            
            _event?.Invoke();
            OnClick();
        }

        public override void OnClick(){}
    }

}
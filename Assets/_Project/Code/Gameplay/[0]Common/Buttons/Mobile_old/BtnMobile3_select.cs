
using System.Collections;
using Galactic1;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Galactic1.Mobile
{
    public class BtnMobile3_select : CoreBtn, IPointerClickHandler
    {
        
        // изменение и сохранения размера до отмены

        
        
        public float duration = .18f;

        public DFuncBoolOut onSelect;
        
        


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
                OnClick();
            }
        }

        public override void OnClick()
        {
            onSelect.Invoke(out bool select);
            if (select)
                Select();
        }


        public void Select()
        {
            StopAllCoroutines();
            StartCoroutine(e(true));
        }

        public void CancelSelect()
        {
            if(gameObject.activeInHierarchy)
            {
                StopAllCoroutines();
                StartCoroutine(e(false));
            }
        }
        

        IEnumerator e(bool select)
        {
            float time = 0;
            Vector2 scale = transform.localScale;
            Vector2 newScale =  select ? Vector2.one * 1.12f : Vector2.one;
            while (time < duration)
            {
                scale = Vector2.Lerp(scale, newScale, time / duration);
                time += Time.deltaTime;
                transform.localScale = scale;
                yield return null;
            }
            transform.localScale = scale;
            //DLog.Alert("Complete");
            //ScreenProfiler.AddMessage("BTN coroutine complete!");
        }
    }

}
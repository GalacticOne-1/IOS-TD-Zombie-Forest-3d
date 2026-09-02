using System.Collections;
using Galactic1.Items;
using UnityEngine;

namespace Galactic1.Code.UI.Tooltips
{
    public abstract class TooltipUI : MonoBehaviour
    {
        
        [SerializeField] protected CanvasGroup group;
        [SerializeField] protected RectTransform root;

        protected Canvas canvas;
        protected RectTransform canvasRect;
        protected RectTransform anchor;

        
        
        IEnumerator Start()
        {
            yield return new WaitForSeconds(1);
            canvas = GetComponentInParent<Canvas>();
            canvasRect = canvas.GetComponent<RectTransform>();
            Hide();
        }



        
        
        
        public virtual void Launch(RectTransform anchor, float showDelay, object data)
        {
            this.anchor = anchor;
            root.anchoredPosition = new(0,5000);
            StartCoroutine(loadDataComplete(showDelay));
        }

        protected abstract IEnumerator loadDataComplete(float showDelay);

        /// <summary>
        /// Показывает подсказку рядом с объектом
        /// </summary>
        protected virtual void Show()
        {
            group.alpha = 1;
        }

        public virtual void Hide()
        {
            StopAllCoroutines();
            group.alpha = 0;
        }
    }

}
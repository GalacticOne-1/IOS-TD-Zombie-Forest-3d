
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Galactic1.Mobile
{
    public abstract class RequireBtn : CoreBtn, IPointerClickHandler
    {
        
        // изменение размера

        [SerializeField, Header("Изменения размера")]
        private float smooth = .1f;
        private Vector2 size;

        public GameObject dark;
        public DFuncResponse onCheck;


        
        
        
        // убирает темный спрайт, показывая что кнопка стала активной
        public void Activate()
        {
            dark.SetActive(false);
        }
        
        
        

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Available()) return;
            
            StopAllCoroutines();
            StartCoroutine(e());
        }

        IEnumerator e()
        {
            for (float i = 1; i > .7f; i-= smooth)
            {
                yield return null;
                size.x = size.y = i;
                gameObject.transform.localScale = size;
            }
            
            yield return new WaitForSeconds(.1f);
            
            for (float i = size.x; i < 1; i+= smooth)
            {
                yield return null;
                size.x = size.y = i;
                gameObject.transform.localScale = size;
            }

            size.x = size.y = 1;
            gameObject.transform.localScale = size;
            
            Click();
        }

        
        /// <summary>
        /// Для проверки доступа кнопки
        /// </summary>
        /// <returns></returns>
        protected abstract bool Available();

        /// <summary>
        /// События при клике
        /// </summary>
        protected abstract void Click();
    }

}
using System;
using System.Collections;
using Galactic1;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Galactic1.Mobile
{
    public class BtnMobile1 : CoreBtn, IPointerClickHandler
    {
        
        [Header("Изменение цвета")]
        // универсально для текста или спрайта

        [SerializeField] private TextMeshProUGUI val;
        [SerializeField] private Image hl, icon;
        [SerializeField] private CColorBtn setColor;
        
        
        private Color def;
        private Color require;
        
        
        
        
        
        private void Awake()
        {
            def = hl.color;
        }


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
                //require = setColor.useSelfColor ? setColor.color : ServiceLocator.Current.Get<IconHub>().highlightUI;
                StopAllCoroutines();
                StartCoroutine(e());
            }
        }

        IEnumerator e()
        {
            hl.color = require;
            if (icon) icon.color = require;
            if (val) val.color = require;
            
            yield return new WaitForSeconds(.2f);
            
            hl.color = def;
            if (icon) icon.color = def;
            if (val) val.color = def;
            
            OnClick();
        }

        public override void OnClick()
        {
            _event?.Invoke();
        }
    }

    [System.Serializable]
    public struct CColorBtn
    {
        [Header("true - для своего цвета")] 
        public bool useSelfColor;
        public Color color;
    }
}

using System;
using Galactic1.Code.UI.Common.Effects;
using Galactic1.Game.Meta.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.RaidLoot
{
    public sealed class LootRewardCard : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _amount;
        

        [SerializeField] private CanvasGroup _canvas;

        
        private UIFadeComponent _fade;
        private IUIFlashEffect _flash;
        
        private void Awake()
        {
            _fade = GetComponent<UIFadeComponent>();
            _fade.Setup();
            
            _flash = GetComponent<IUIFlashEffect>();
        }
        
        
        public void Show(ItemConfig item, int amount)
        {
            root.SetActive(false);
            _title.text = item.Header.titleLid;
            _amount.text = $"x{amount}";

            gameObject.SetActive(true);
            _fade.SetInstant(0f);
            _fade.FadeIn(false,() =>
            {
                // 1. Flash shader
                _flash.Play(() =>
                {
                    // 2. Enable root AFTER flash
                    root.SetActive(true);
                });
            });
        }
        
        public void Hide(Action onComplete = null)
        {
            root.SetActive(false);

            _flash.Play(() =>
            {
                _fade.FadeOut(onComplete, .5f);
            });
        }
        
    }
}
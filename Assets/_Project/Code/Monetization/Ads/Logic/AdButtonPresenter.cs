
using System.Collections;
using Galactic1.Code.Core.Ads;
using Galactic1.Code.Systems.Ads;
using Galactic1.Configs;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;

namespace Galactic1.Code.UI.Ads
{
    /// <summary>
    /// Presenter рекламной кнопки.
    /// UI ничего не знает о SDK.
    /// </summary>
    public class AdButtonPresenter : BaseUIButton
    {
        
        [SerializeField] private AdPlacement placement;
        [SerializeField] private AdFormat format;
        [SerializeField] private TMP_Text adLimitText;
        
        
        private AdService adService;
        private AdDecision currentDecision;




        protected virtual void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            if (adService != null)
            {
                adService.OnAdDecisionChanged -= OnDecisionChanged;
            }
        }

        protected void Initialize()
        {
            if (ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>().requiresAdService)
            {
                adService = ServiceLocator.Current.Get<AdService>();

                adService.OnAdDecisionChanged += OnDecisionChanged;
                EventBus<SceneServicesClearEvent>.Register(new EventBinding<SceneServicesClearEvent>(() =>
                    adService.OnAdDecisionChanged -= OnDecisionChanged));

                // сразу инициализируем
                OnDecisionChanged(adService.Policy.Evaluate());
            }
            else
            {
                AdStatus(false);
            }
        }
        
        

        private void OnDecisionChanged(AdDecision decision)
        {
            currentDecision = decision;

            // Управление кнопкой
            AdStatus(currentDecision.Allowed);
                
            // обновляем лимит
            if (adLimitText != null)
            {
                adLimitText.text = $"{adService.Economy.RemainingLimit}/{adService.Economy.DailyLimitConfig}";
            }
        }
        
        
        

        protected override bool HandleClick()
        {
            if (onLock?.Invoke() ?? false)
                return false;
            
            if (!base.HandleClick() || adService == null)
            {
                AdUtility.NotAvailable();
                return false;
            }
            
            // ========= AD ==========================================================================================
            
            currentDecision = adService.Policy.Evaluate();
            
            // Если реклама доступна
            if (currentDecision.Allowed)
            {
                StartCoroutine(ShowAd());
            }
            else
            {
                // Показываем причину недоступности
                if (currentDecision.CooldownRemaining > 0)
                    AdUtility.Break(currentDecision.CooldownRemaining);
                else
                    AdUtility.NotAvailable();
            }

            return true;
        }

        private IEnumerator ShowAd()
        {
            var task = adService.TryShowAsync(placement, format);

            while (!task.IsCompleted)
                yield return null;

            var result = task.Result;

            if (!result.Allowed)
                AdUtility.NotAvailable();
        }
    }
}
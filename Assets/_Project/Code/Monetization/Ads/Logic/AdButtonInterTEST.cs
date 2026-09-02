
using Galactic1.Code.Core.Ads;
using Galactic1.Code.Systems.Ads;
using Galactic1.Configs;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.UI.Ads
{
    public class AdButtonInterTEST : BaseUIButton
    {
        private void OnEnable()
        {
            events.onClick.AddListener(OnClick);
        }

        public async void OnClick()
        {
            if (!ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>().requiresAdService) return;
            var adService = ServiceLocator.Current.Get<AdService>();

            var decision = await adService.TryShowAsync(AdPlacement.PostLevelInterstitial, AdFormat.Interstitial);
    
            if (!decision.Allowed)
                Debug.Log($"Interstitial not shown: {decision.Reason}");
        }
    }
}
using System.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Galactic1.Code.Systems.Ads.AdMob
{
    /// <summary>
    /// Управляет interstitial рекламой.
    /// </summary>
    public class AdMobInterstitialHandler
    {
        private InterstitialAd interstitialAd;
        private readonly string adUnitId;

        public AdMobInterstitialHandler(string adUnitId)
        {
            this.adUnitId = adUnitId;
        }

        public async Task LoadAsync()
        {
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }

            var tcs = new TaskCompletionSource<bool>();
            var request = new AdRequest();

            InterstitialAd.Load(adUnitId, request, (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"Interstitial load failed: {error}");
                    tcs.TrySetResult(false);
                    return;
                }

                interstitialAd = ad;
                tcs.TrySetResult(true);
            });

            await tcs.Task;
        }

        public bool IsReady() => interstitialAd != null && interstitialAd.CanShowAd();

        public Task<bool> ShowAsync()
        {
            if (!IsReady())
                return Task.FromResult(false);

            var tcs = new TaskCompletionSource<bool>();

            interstitialAd.OnAdFullScreenContentClosed += () => tcs.TrySetResult(true);
            interstitialAd.OnAdFullScreenContentFailed += _ => tcs.TrySetResult(false);

            interstitialAd.Show();
            return tcs.Task;
        }
    }
}
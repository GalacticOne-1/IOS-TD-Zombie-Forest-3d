
using System.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Galactic1.Code.Systems.Ads.AdMob
{
    /// <summary>
    /// Управляет жизненным циклом rewarded рекламы.
    /// </summary>
    public class AdMobRewardedHandler
    {
        private RewardedAd rewardedAd;
        private readonly string adUnitId;

        public AdMobRewardedHandler(string adUnitId)
        {
            this.adUnitId = adUnitId;
        }

        public async Task LoadAsync()
        {
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }

            var tcs = new TaskCompletionSource<bool>();
            var request = new AdRequest();

            RewardedAd.Load(adUnitId, request, (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"Rewarded load failed: {error}");
                    tcs.TrySetResult(false);
                    return;
                }

                rewardedAd = ad;
                RegisterCallbacks(ad);
                tcs.TrySetResult(true);
            });

            await tcs.Task;
        }

        public bool IsReady() => rewardedAd != null && rewardedAd.CanShowAd();

        public Task<bool> ShowAsync()
        {
            if (!IsReady())
                return Task.FromResult(false);

            var tcs = new TaskCompletionSource<bool>();

            rewardedAd.Show(reward =>
            {
                Debug.Log($"Reward earned: {reward.Amount}");
            });

            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                tcs.TrySetResult(true);
            };

            rewardedAd.OnAdFullScreenContentFailed += err =>
            {
                Debug.LogError(err);
                tcs.TrySetResult(false);
            };

            return tcs.Task;
        }

        private void RegisterCallbacks(RewardedAd ad)
        {
            ad.OnAdFullScreenContentOpened += () => Debug.Log("Rewarded opened");
            ad.OnAdImpressionRecorded += () => Debug.Log("Rewarded impression");
            ad.OnAdClicked += () => Debug.Log("Rewarded clicked");
        }
    }
}

using System.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Galactic1.Code.Systems.Ads.AdMob
{
    /// <summary>
    /// Глобальный сервис состояния SDK AdMob.
    /// Инициализируется один раз при старте игры.
    /// </summary>
    public class AdMobInitializer
    {
        public bool IsInitialized { get; private set; }

        public async Task InitializeAsync()
        {
            if (IsInitialized)
                return;

            var tcs = new TaskCompletionSource<bool>();

            MobileAds.Initialize(initStatus =>
            {
                Debug.Log("AdMob SDK Initialized");
                IsInitialized = true;
                tcs.SetResult(true);
            });

            await tcs.Task;
        }
    }
}
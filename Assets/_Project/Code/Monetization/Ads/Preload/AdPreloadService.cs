using System;
using System.Collections;
using System.Collections.Generic;
using Galactic1.Code.Core.Ads;
using Galactic1.Code.Systems.Ads.AdMob;
using Galactic1.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Централизованно управляет preload рекламных placement'ов.
    /// Работает фоново и автономно.
    /// </summary>
    public class AdPreloadService
    {
        private readonly IAdNetworkAdapter adapter;
        private readonly AdMobInitializer sdk;
        private readonly Coroutines coroutineHost;

        public event Action OnAdLoaded;

        private readonly Dictionary<AdPlacement, AdFormat> placements = new()
        {
            { AdPlacement.GameShop1, AdFormat.Rewarded },
            { AdPlacement.PostLevelInterstitial, AdFormat.Interstitial }
        };

        private readonly float retryDelay = 10f;

        public AdPreloadService(
            IAdNetworkAdapter adapter,
            AdMobInitializer sdk,
            Coroutines coroutineHost)
        {
            this.adapter = adapter;
            this.sdk = sdk;
            this.coroutineHost = coroutineHost;
        }

        public void Start()
        {
            coroutineHost.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            // 1. Ждём SDK
            while (!sdk.IsInitialized)
                yield return null;

            // 2. Первичная загрузка
            foreach (var kvp in placements)
                coroutineHost.StartCoroutine(PreloadLoop(kvp.Key, kvp.Value));
        }

        private IEnumerator PreloadLoop(AdPlacement placement, AdFormat format)
        {
            while (true)
            {
                if (!adapter.IsReady(format))
                {
                    var task = adapter.LoadAsync(format);
                    while (!task.IsCompleted)
                        yield return null;

                    if (adapter.IsReady(format))
                        OnAdLoaded?.Invoke();
                }

                yield return new WaitForSeconds(retryDelay);
            }
        }


    }
}

using System;
using System.Collections;
using Galactic1.Code.Systems.Ads;
using Galactic1.Code.Systems.Ads.AdMob;
using Galactic1.Core;
using Galactic1.Mobile;
using Galactic1.Systems.Privacy;
using UnityEngine;

namespace Galactic1.Systems.Ads
{
    /// <summary>
    /// Фоновая инициализация рекламы. Игру не блокирует.
    /// Старт SDK: сразу, если CanRequestAds() уже true, иначе после завершения UMP.
    /// </summary>
    public class AdsBootstrap
    {
        private readonly Coroutines _coroutines;
        private readonly DIContainer _root;
        private readonly GameConfig _config;

        private bool _started;
        private bool _sdkInitStarted;

        public bool IsReady { get; private set; }
        public event Action Ready;

        public AdsBootstrap(DIContainer root, Coroutines coroutines, GameConfig config)
        {
            _root = root;
            _coroutines = coroutines;
            _config = config;
        }

        public void Start()
        {
            if (_started) return;
            _started = true;

            var consent = new ConsentService();
            ServiceLocator.Current.Register(consent);

            // Значение с прошлой сессии: Google разрешает опираться на него до Update()
            var canRequestNow = ConsentController.CanRequestAds;

            if (canRequestNow)
                StartSdk();

            // Сбор/обновление согласия идёт параллельно с игрой.
            // В быстром пути ConsentController сам вызовет Update() и сразу завершится.
            consent.GatherConsent(_config, () =>
            {
                Debug.Log($"UMP processed, CanRequestAds={consent.CanRequestAds}");

                if (!canRequestNow)
                    StartSdk(); // UMP обработан (успешно или с ошибкой), реклама запускается в любом случае
            });
        }

        private void StartSdk()
        {
            if (_sdkInitStarted) return;
            _sdkInitStarted = true;
            _coroutines.StartCoroutine(InitSdk());
        }

        private IEnumerator InitSdk()
        {
            var sdk = new AdMobInitializer();
            var task = sdk.InitializeAsync();

            while (!task.IsCompleted)
                yield return null;

            var adapter = new AdMobAdapter();
            var preload = new AdPreloadService(adapter, sdk, _coroutines);
            preload.Start();

            var adService = AdInstaller.Create(_root, _coroutines, adapter, preload);
            ServiceLocator.Current.Register(adService);

            IsReady = true;
            Ready?.Invoke();
        }
    }
}
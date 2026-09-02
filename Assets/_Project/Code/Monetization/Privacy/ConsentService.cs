using System;
using Galactic1.Mobile;
using UnityEngine;

namespace Galactic1.Systems.Privacy
{
    /// <summary>
    /// Управляет получением GDPR/UMP согласия.
    /// Ничего не знает о рекламе.
    /// </summary>
    public class ConsentService : IGameService
    {
        public bool CanRequestAds { get; private set; }

        public void GatherConsent(GameConfig config, Action onComplete)
        {
            var controller = new ConsentController();

            controller.GatherConsent(config, (error, canRequestAds) =>
            {
                if (error != null)
                    Debug.LogWarning($"GDPR error: {error}");

                CanRequestAds = canRequestAds;
                onComplete?.Invoke();
            });
        }

        public void ShowPrivacyOptions(Action<string> onError)
        {
            new ConsentController().ShowPrivacyOptionsForm(onError);
        }
    }
}
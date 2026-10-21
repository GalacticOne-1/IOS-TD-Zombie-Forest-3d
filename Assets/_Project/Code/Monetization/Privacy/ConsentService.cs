using System;
using Galactic1.Mobile;
using UnityEngine;

namespace Galactic1.Systems.Privacy
{
    public class ConsentService : IGameService
    {
        private readonly ConsentController _controller = new();

        public bool CanRequestAds { get; private set; }
        public bool PrivacyOptionsRequired => ConsentController.PrivacyOptionsRequired;

        public void GatherConsent(GameConfig config, Action onComplete)
        {
            _controller.GatherConsent(config, (error, canRequestAds) =>
            {
                if (!string.IsNullOrEmpty(error))
                    Debug.Log($"GDPR: {error}");

                CanRequestAds = canRequestAds;
                onComplete?.Invoke();
            });
        }

        /// <summary>Для кнопки в настройках. После закрытия формы CanRequestAds обновляется.</summary>
        public void ShowPrivacyOptions(Action<string> onComplete = null)
        {
            _controller.ShowPrivacyOptionsForm(error =>
            {
                CanRequestAds = ConsentController.CanRequestAds;
                onComplete?.Invoke(error);
            });
        }
    }
}
using System;
using System.Collections.Generic;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace Galactic1.Mobile
{
    public class ConsentController
    {
        // Для теста: форсировать EEA в dev-билдах и добавить hashed id устройства из логов
        private const bool FORCE_EEA_IN_DEBUG = false;
        private static readonly List<string> TestDeviceIds = new() { /* "HASHED_ID_FROM_LOGCAT" */ };

        public static bool CanRequestAds => ConsentInformation.CanRequestAds();

        public static bool PrivacyOptionsRequired =>
            ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

        /// <summary>
        /// Вызывать ДО инициализации рекламы (после ATT на iOS).
        /// onComplete вызывается ВСЕГДА ровно один раз: (error или null, итоговый canRequestAds).
        /// </summary>
        public void GatherConsent(GameConfig config, Action<string, bool> onComplete)
        {
            var finished = false;

            void Finish(string error)
            {
                if (finished) return;
                finished = true;
                // читаем актуальное значение именно сейчас, после всех шагов
                onComplete?.Invoke(error, ConsentInformation.CanRequestAds());
            }

            var request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
            };

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (FORCE_EEA_IN_DEBUG)
            {
                request.ConsentDebugSettings = new ConsentDebugSettings
                {
                    DebugGeography = DebugGeography.EEA,
                    TestDeviceHashedIds = TestDeviceIds,
                };
            }
#endif

            // Согласие уже есть с прошлой сессии: не заставляем игрока ждать сеть,
            // но статус обновляем (форма покажется на следующем запуске, если согласие устарело).
            if (ConsentInformation.CanRequestAds())
            {
                ConsentInformation.Update(request, _ => { });
                Finish(null);
                return;
            }

            // #1 обновляем статус согласия (на каждом запуске, как требует Google)
            ConsentInformation.Update(request, updateError =>
            {
                if (updateError != null)
                {
                    Finish($"UMP update: {updateError.Message}");
                    return;
                }

                // #2 показываем форму, только если она требуется (EEA/UK и т.п.)
                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    // и при ошибке, и при успехе доходим до Finish
                    Finish(formError?.Message);
                });
            });
        }

        /// <summary>Форма настроек приватности (для кнопки в настройках).</summary>
        public void ShowPrivacyOptionsForm(Action<string> onComplete)
        {
            ConsentForm.ShowPrivacyOptionsForm(showError => onComplete?.Invoke(showError?.Message));
        }
    }
}
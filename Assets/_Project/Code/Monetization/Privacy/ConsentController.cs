using System;
using Galactic1;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace Galactic1.Mobile
{
    public class ConsentController
    {
        
        
        
        
        
        
        /// <summary>
        /// Вызов в ApplicationSetup before AD Init
        /// </summary>
        public void GatherConsent(GameConfig config,  Action<string, bool> onComplete)
        {
            
            var canRequestAds = ConsentInformation.CanRequestAds();
            
#if UNITY_IOS && !UNITY_EDITOR
            
            // *** нет согласия по ATT, форму согласия не запускаеем
            if(config.Ios.statusATT != 3)
            {
                onComplete.Invoke("Ios ATT is false ", canRequestAds);
                return;
            }
            
#elif UNITY_ANDROID && !UNITY_EDITOR
            // проверка разрешения для андроида
#endif

            
            // *** Если согласие получено в предыдущих сессиях ***************************************
            if (ConsentInformation.CanRequestAds())
            {
                onComplete.Invoke("Consent previous session", true);
                return;
            }
            
            
            
            // *** Для нового согласия  ***************************************************************
            
            // Set tag for under age of consent.
            // Here false means users are not under age of consent.
            ConsentRequestParameters request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
            };

            
            // #1 Check the current consent information status.
            ConsentInformation.Update(request, (FormError updateError) =>
            {
                // Handle the error.
                if (updateError != null)
                {
                    onComplete(updateError.Message, canRequestAds);
                    return;
                }
                
                // *** Enable the privacy basicSettings button.
                // if (ServiceLocator.Current.Get<W_Options>().buttonsFeature.bConsentOption)
                // {
                //     ServiceLocator.Current.Get<W_Options>().buttonsFeature.bConsentOption.gameObject
                //         .SetActive(ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required);
                // }
                
                
                // #2 If the error is null, the consent information state was updated.
                // You are now ready to check if a form is available.
                ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
                {
                    if (formError != null)
                    {
                        // Consent gathering failed.
                        ScreenProfiler.AddMessage("GDPR : Consent gathering failed".SetText(EDlogColor.ORANGE));
                        return;
                    }
                
                    // Consent gathering process has completed. (new consent)
                    onComplete("Consent new session | "+formError?.Message, true);
                });
            });
            
        }
        
        
        /// <summary>
        /// Shows the privacy options form to the user. (for button)
        /// </summary>
        public void ShowPrivacyOptionsForm(Action<string> onComplete)
        {
            ConsentForm.ShowPrivacyOptionsForm((FormError showError) =>
            {
                onComplete?.Invoke(showError?.Message);
            });
        }
    }
}
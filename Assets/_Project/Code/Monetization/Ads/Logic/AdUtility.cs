
using Galactic1.Code.Notification;
using Galactic1.Core.Results;
using UnityEngine;

namespace Galactic1.Code.UI.Ads
{
    public static class AdUtility
    {
        
        public static void NotAvailable()
        {
            // ServiceLocator.Current.Get<UIManager>().OpenPopup(
            //     UIScreenId.AdAlertToast,
            //     ServiceLocator.Current.Get<LocalisationService>().Data.ad_disabled);
            ServiceLocator.Current.Get<INotificationService>().Push(NotificationFailReason.AdNotAvailable);
        }

        public static void Break(float cooldown)
        {
            // var s = ServiceLocator.Current.Get<LocalisationService>().Data.ad_restoring;
            // s += $" {Mathf.CeilToInt(cooldown)}s";
            // ServiceLocator.Current.Get<UIManager>().OpenPopup(UIScreenId.AdAlertToast, s);
            ServiceLocator.Current.Get<INotificationService>()
                .Push(NotificationFailReason.AdBreak, $"{Mathf.CeilToInt(cooldown)}s");
        }
    }
}
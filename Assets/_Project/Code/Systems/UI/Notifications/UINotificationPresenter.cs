
using Galactic1.Code.Notification;
using Galactic1.Core.Notifications;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.UI.Notifications
{
    public sealed class UINotificationPresenter : MonoBehaviour
    {
        [SerializeField] private ToastManager toastManager;
        [SerializeField] private UIPopupManager popupManager;

        private INotificationService _service;

        
        
        
        public void Initialize(INotificationService service)
        {
            _service = service as NotificationService;
            
            toastManager?.Initialize(_service);

            _service.OnDispatch += Handle;
        }

        private void Handle(NotificationRequest request)
        {
            DLog.Alert("Toast");
            switch (request.Channel)
            {
                case NotificationChannel.Toast:
                    toastManager.Show(request);
                    break;

                case NotificationChannel.Popup:
                    popupManager.OpenPopup(UIScreenId.ConfirmPopup, request.Message);
                    break;

                case NotificationChannel.Banner:
                    // bannerManager.Show(...)
                    break;
            }
        }
    }
}
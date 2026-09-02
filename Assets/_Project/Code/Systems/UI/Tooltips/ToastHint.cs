
using Galactic1.Code.Notification;
using Galactic1.Core.Results;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.UI.Tooltips
{
    /// <summary>
    /// Кнопка для вызова простой подсказки
    /// </summary>
    public class ToastHint : BaseUIButton
    {
        [SerializeField] private NotificationFailReason reason;


        private void Start()
        {
            gameObject.RegisterButtonClick(() =>
                ServiceLocator.Current.Get<INotificationService>().Push(reason));
        }
    }
}
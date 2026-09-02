using System;
using Galactic1.Core.Notifications;
using Galactic1.Core.Results;

namespace Galactic1.Code.Notification
{
    public interface INotificationService : IGameService
    {
        event Action<NotificationRequest> OnDispatch;

        
        
        /// <summary>
        /// Тост с кастомным текстом
        /// </summary>
        /// <param name="message"></param>
        void Push(int id, string message, NotificationStyleCategory style = NotificationStyleCategory.Default);
        
        /// <summary>
        /// Для своего тоста (свой месседж)
        /// </summary>
        /// <param name="request"></param>
        void Push(NotificationRequest request);
        
        /// <summary>
        /// Для технического тоста (из конфига)
        /// </summary>
        /// <param name="reason"></param>
        void Push(NotificationFailReason reason, string extra = "");
    }
}
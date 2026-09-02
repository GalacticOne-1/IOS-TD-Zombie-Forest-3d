
using Galactic1.Code.Notification;

namespace Galactic1.Core.Notifications
{
    /// <summary>
    /// Унифицированная модель уведомления.
    /// </summary>
    public sealed class NotificationRequest
    {
        public string Id { get; }
        public string Message { get; }
        
        public NotificationPriority Priority { get; }
        public NotificationChannel Channel { get; }
        public float Duration { get; }
        public bool AllowDuplicate { get; }
        
        
        public NotificationStyleCategory StyleCategory { get; }
        public NotificationMessageConfig.NotificationStyle Style { get; set; }
        

        public NotificationRequest(
            string id,
            string message,
            NotificationPriority priority,
            NotificationChannel channel,  
            NotificationStyleCategory styleCategory,
            NotificationMessageConfig.NotificationStyle style,
            float duration = 2f,
            bool allowDuplicate = false)
        {
            Id = id;
            Message = message;
            StyleCategory = styleCategory;
            Style = style;
            Priority = priority;
            Channel = channel;
            Duration = duration;
            AllowDuplicate = allowDuplicate;
        }
    }
}
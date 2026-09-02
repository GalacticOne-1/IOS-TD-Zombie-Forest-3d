
using System;
using System.Collections.Generic;
using Galactic1.Core.Notifications;
using Galactic1.Core.Results;

namespace Galactic1.Code.Notification
{
    /// <summary>
    /// Live-service уровень NotificationSystem.
    /// Управляет очередями, приоритетами, дедупликацией.
    /// UI подписывается на OnDispatch.
    /// </summary>
    public sealed class NotificationService : INotificationService
    {
        private readonly NotificationMessageConfig _config;
        
        public event Action<NotificationRequest> OnDispatch;

        private readonly SortedDictionary<int, Queue<NotificationRequest>> _queues = new();

        private readonly HashSet<string> _activeIds = new();

        private bool _isProcessing;

        public NotificationService(NotificationMessageConfig config)
        {
            _config = config;
            foreach (NotificationPriority p in Enum.GetValues(typeof(NotificationPriority)))
            {
                _queues[(int)p] = new Queue<NotificationRequest>();
            }
        }

        
        
        
        public void Push(int id, string message, NotificationStyleCategory style = NotificationStyleCategory.Default)
        {
            var request = new NotificationRequest(
                id.ToString(),
                message,
                NotificationPriority.Normal,
                NotificationChannel.Toast,
                style,
                _config.GetStyle(style));

            Push(request);
        }
        
        public void Push(NotificationRequest request)
        {
            if (!request.AllowDuplicate && _activeIds.Contains(request.Id))
                return;

            // * заполяем стиль если пусто
            if (request.Style.Category == NotificationStyleCategory.None)
            {
                request.Style = _config.GetStyle(request.StyleCategory);
            }

            _queues[(int)request.Priority].Enqueue(request);
            Process();
        }
        
        public void Push(NotificationFailReason reason, string extra = "")
        {
            if (!_config.TryGet(reason, out var entry))
            {
                GConsole.ClearLog();
                DLog.Alert($"NotificationService not config for: {reason}", EDlogColor.ORANGE);
                return;
            }

            if (!string.IsNullOrEmpty(extra))
                entry.Message += $" {extra}";

            var request = new NotificationRequest(
                entry.Id,
                entry.Message,
                NotificationPriority.Normal,
                NotificationChannel.Toast,
                entry.StyleCategory,
                entry.Style);

            Push(request);
        }
        

        private void Process()
        {
            //if (_isProcessing)
                //return;

            var next = DequeueHighest();
            if (next == null)
                return;

            _isProcessing = true;
            _activeIds.Add(next.Id);

            OnDispatch?.Invoke(next);
        }

        public void Complete(NotificationRequest request)
        {
            _activeIds.Remove(request.Id);
            _isProcessing = false;
            Process();
        }

        private NotificationRequest DequeueHighest()
        {
            for (int i = 3; i >= 0; i--)
            {
                if (_queues[i].Count > 0)
                    return _queues[i].Dequeue();
            }

            return null;
        }
    }
}
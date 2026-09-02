
namespace Galactic1.Core.Results
{
    /// <summary>
    /// Унифицированный результат выполнения операции.
    /// Используется всеми Runtime-сервисами.
    /// </summary>
    public readonly struct NotificationResult
    {
        public bool Success { get; }
        public NotificationFailReason FailReason { get; }

        private NotificationResult(bool success, NotificationFailReason reason)
        {
            Success = success;
            FailReason = reason;
        }

        public static NotificationResult Ok()
            => new NotificationResult(true, NotificationFailReason.None);

        public static NotificationResult Fail(NotificationFailReason reason)
            => new NotificationResult(false, reason);
    }
}
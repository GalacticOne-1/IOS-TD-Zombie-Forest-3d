
namespace Galactic1.Code.Systems.Tutorial.Notification
{
    /// <summary>
    /// Notification id для tutorial target gate toast (см. ITutorialTargetGateService).
    ///
    /// Не пересекается с NotificationFailReason: тот enum используется ТОЛЬКО через
    /// Push(NotificationFailReason, string) — там NotificationRequest.Id строится из
    /// NotificationMessageConfig-entry.Id (произвольная string, из конфига), а не из этой
    /// константы. Push(int id, string message, ...) — отдельный путь (см. NotificationService.
    /// Push(int, string, ...)), id там используется только как request.Id.ToString() для
    /// dedup-ключа в HashSet<string> _activeIds. Разные типы (int vs enum) — коллизия
    /// физически невозможна.
    ///
    /// Единая константа для ВСЕХ target gate toast'ов — намеренно: пока один блокирующий
    /// toast активен (не завершён через NotificationService.Complete), повторный клик по
    /// любому заблокированному target (тому же или другому) дедуплицируется существующим
    /// AllowDuplicate-механизмом NotificationService.Push, вместо стека параллельных
    /// уведомлений (см. edge case "Повторный клик" в ТЗ).
    /// </summary>
    public static class TutorialNotificationIds
    {
        public const int TargetGateBlocked = 9001;
    }
}
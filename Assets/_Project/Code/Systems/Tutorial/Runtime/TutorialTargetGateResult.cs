// Path: /Code/Systems/Tutorial/Runtime/ITutorialTargetGateService.cs
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>Результат проверки gate для одного target. IsAllowed=true означает
    /// "клик может выполнять свою обычную логику дальше" — сервис не открывает окон,
    /// не показывает Toast и не знает о UI/сценах, это ответственность вызывающего
    /// gameplay-кода (HUDCamp/FacilityInstance).</summary>
    public readonly struct TutorialTargetGateResult
    {
        public readonly bool IsAllowed;
        public readonly string BlockedMessage;

        private TutorialTargetGateResult(bool isAllowed, string blockedMessage)
        {
            IsAllowed = isAllowed;
            BlockedMessage = blockedMessage;
        }

        public static readonly TutorialTargetGateResult Allowed = new(true, null);
        public static TutorialTargetGateResult Blocked(string message) => new(false, message);
    }

    /// <summary>
    /// Единственная точка проверки "разрешён ли клик по target прямо сейчас". Не открывает
    /// UI, не отправляет события, не знает о конкретных кнопках/зданиях/сценах — чистая
    /// query-функция над TutorialTargetGateRegistry + ITutorialService.
    /// </summary>
    public interface ITutorialTargetGateService : IGameService
    {
        TutorialTargetGateResult Evaluate(TutorialTargetId targetId);
    }
}
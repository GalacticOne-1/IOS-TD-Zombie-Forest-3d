
using System;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Одна authoring-запись tutorial-блокировки клика: связывает произвольный target
    /// (UI-кнопка или world-объект, идентифицированный TutorialTargetId — тем же
    /// стабильным id, что уже используется TutorialTargetRegistry/
    /// WorldTutorialTargetBehaviour) с шагом tutorial, который должен быть завершён,
    /// прежде чем клик по target будет пропущен к своей обычной логике.
    ///
    /// Soft Launch: один target → один requiredStepId (AND/OR не поддержаны намеренно —
    /// см. TutorialTargetGateService докстринг про будущее расширение).
    ///
    /// blockedMessage — авторский текст, а не ссылка на TutorialStepDefinition.presentation.
    /// instructionTitleKey: нет единого способа резолвить TutorialStepDefinition только по
    /// TutorialStepId без знания кампании (TutorialDefinition.GetStep требует конкретный
    /// campaign asset), поэтому текст блокировки дублируется здесь явно, авторится вместе
    /// с самим gate — не hardcode в gameplay-коде (HUDCamp/FacilityInstance его не видят).
    /// </summary>
    [Serializable]
    public sealed class TutorialTargetGateDefinition
    {
        [Tooltip("Target, клик по которому нужно перехватывать...")]
        public TutorialTargetId targetId;

        [Tooltip("Шаг tutorial, который должен быть завершён...")]
        public TutorialStepId requiredStepId;

        [Tooltip("true = текст блокировки строится из заголовка requiredStepId " +
                 "(TutorialStepDefinition.presentation.instructionTitleKey через " +
                 "messageTemplate) — blockedMessage игнорируется.\n" +
                 "false = используется blockedMessage как есть.")]
        public bool useStepTitle;

        [Tooltip("Используется только при useStepTitle=true. {0} подставляется заголовком " +
                 "шага. Пример: 'Сначала выполните: {0}'.")]
        public string messageTemplate = "Сначала выполните: {0}";

        [Tooltip("Игнорируется при useStepTitle=true. Фиксированный текст Toast.")] [TextArea]
        public string blockedMessage;

#if UNITY_EDITOR
        public bool Validate(out string error)
        {
            if (targetId == null)
            {
                error = "TutorialTargetGateDefinition: targetId is empty.";
                return false;
            }

            if (requiredStepId == null)
            {
                error = $"TutorialTargetGateDefinition (target '{targetId.DebugKey}'): requiredStepId is empty.";
                return false;
            }

            if (useStepTitle)
            {
                if (string.IsNullOrEmpty(messageTemplate))
                {
                    error = $"TutorialTargetGateDefinition (target '{targetId.DebugKey}'): " +
                            "useStepTitle=true но messageTemplate пуст.";
                    return false;
                }

                if (!messageTemplate.Contains("{0}"))
                {
                    error = $"TutorialTargetGateDefinition (target '{targetId.DebugKey}'): " +
                            "messageTemplate не содержит {0} — заголовок шага никуда не подставится.";
                    return false;
                }
            }
            else if (string.IsNullOrEmpty(blockedMessage))
            {
                error = $"TutorialTargetGateDefinition (target '{targetId.DebugKey}'): " +
                        "blockedMessage is empty (или включи useStepTitle).";
                return false;
            }

            error = null;
            return true;
        }
#endif
    }
}
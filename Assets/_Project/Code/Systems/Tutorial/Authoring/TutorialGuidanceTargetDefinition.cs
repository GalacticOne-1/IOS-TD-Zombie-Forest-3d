using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Construction.Configs;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Presentation-данные одного guidance-варианта — Unity-инспекторное подмножество
    /// TutorialPresentationDefinition, ТОЛЬКО highlight/arrow/camera (без instruction/
    /// dialogue/inputPolicy — те остаются на уровне шага, guidance их не переопределяет,
    /// см. TutorialGuidanceDefinition докстринг). Отдельный тип, а не переиспользование
    /// TutorialPresentationDefinition напрямую — чтобы в guidance-списке нельзя было
    /// случайно задать разную инструкцию/входную политику для разных guidance-вариантов
    /// одного и того же шага, что архитектурно не имеет смысла.
    ///
    /// highlightTargetId / highlightItemId / highlightFacilityItemId — АЛЬТЕРНАТИВНЫЕ
    /// стратегии резолва highlight: фиксированный UI-элемент (TutorialTargetRegistry) или
    /// "слот/карточка, где сейчас лежит X" (живой resolve на каждый показ через
    /// ITutorialItemSlotTargetProvider / ITutorialFacilitySlotTargetProvider). Взаимоисключимы —
    /// см. Validate.
    /// </summary>
    [Serializable]
    public sealed class TutorialGuidanceTargetDefinition
    {
        
        public HighlightMode highlightMode = HighlightMode.None;
        
        [Tooltip("Взаимоисключимо с highlightItemId. Оставь пустым, если для этого guidance-" +
                 "варианта highlight не нужен, либо используется highlightItemId.")]
        public TutorialTargetId highlightTargetId;

        [Tooltip("Взаимоисключимо с highlightTargetId. Highlight слота, где СЕЙЧАС лежит этот " +
                 "предмет — используй, когда позиция предмета в инвентаре заранее не известна " +
                 "(см. класс-докстринг).")]
        public ItemId highlightItemId;

        [Tooltip("Взаимоисключимо с highlightTargetId и highlightItemId. Highlight кнопки Take " +
                 "во ВХОДЯЩИХ (Inbox) для этого предмета — резолвится ТОЛЬКО через Inbox-провайдер, " +
                 "инвентарь не участвует, даже если тот же предмет уже есть где-то ещё.")]
        public ItemId highlightInboxItemId;
        
        [Tooltip("Взаимоисключимо с highlightTargetId/highlightItemId/highlightInboxItemId. " +
                 "Highlight динамически найденного юнита в списке по критериям — см. " +
                 "TutorialUnitSearchCriteria.")]
        public TutorialUnitSearchCriteria highlightUnitSearch;

        [Tooltip("Взаимоисключимо с остальными highlight*-полями. Highlight карточки здания " +
                 "в панели строительства для этого facility item id — резолвится через " +
                 "ITutorialFacilitySlotTargetProvider (см. её докстринг).")]
        public ItemId highlightFacilityItemId;

        [Tooltip("Взаимоисключимо с остальными highlight*-полями. Highlight кнопки вкладки " +
                 "этой категории в панели строительства — резолвится через " +
                 "ITutorialConstructionTabTargetProvider (см. её докстринг). Nullable по той " +
                 "же причине, что и в TutorialPresentationDefinition — 0 (All) валидная категория.")]
        public ConstructionCategory? highlightConstructionTabCategory;

        [Tooltip("Оставь пустым, если для этого guidance-варианта стрелка не нужна.")]
        public TutorialTargetId arrowTargetId;

        [Tooltip("Оставь пустым, если для этого guidance-варианта фокус камеры не нужен.")]
        public TutorialTargetId cameraFocusTargetId;
        
        

        public bool HasAnyTarget =>
            highlightTargetId != null
            || highlightItemId != null
            || highlightInboxItemId != null
            || highlightUnitSearch != null
            || highlightFacilityItemId != null
            || highlightConstructionTabCategory.HasValue
            || arrowTargetId != null
            || cameraFocusTargetId != null;

#if UNITY_EDITOR
        public bool Validate(out string error)
        {
            int highlightModeCount =
                (highlightTargetId != null ? 1 : 0) +
                (highlightItemId != null ? 1 : 0) +
                (highlightInboxItemId != null ? 1 : 0) +
                (highlightUnitSearch != null ? 1 : 0) +
                (highlightFacilityItemId != null ? 1 : 0) +
                (highlightConstructionTabCategory.HasValue ? 1 : 0) +
                (cameraFocusTargetId != null ? 1 : 0);

            if (highlightModeCount > 1)
            {
                error =
                    "highlightTargetId, highlightItemId, highlightInboxItemId, highlightFacilityItemId и " +
                    "highlightConstructionTabCategory взаимоисключимы — задай только одно.";
                return false;
            }

            error = null;
            return true;
        }
#endif
    }
}
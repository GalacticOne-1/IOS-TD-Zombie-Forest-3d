using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Construction.Configs;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Декларативное описание презентации шага. TutorialPresentationService
    /// интерпретирует эти поля — сам класс не содержит логики.
    /// </summary>
    [Serializable]
    public sealed class TutorialPresentationDefinition
    {
        [Header("Instruction")]
        public string instructionTitleKey;
        [TextArea]
        public string instructionDesKey;
        
        
        public HighlightMode highlightMode = HighlightMode.None;

        [Header("Highlight / Arrow")]
        [Tooltip("Взаимоисключимо с highlightItemId — задавай только одно. Фиксированный " +
                 "UI-элемент (кнопка, статичный слот экипировки и т.п.).")]
        public TutorialTargetId highlightTargetId;

        [Tooltip("Взаимоисключимо с highlightTargetId. Highlight не фиксированного UI-" +
                 "элемента, а слота инвентаря, где СЕЙЧАС лежит указанный предмет — резолвится " +
                 "заново при каждом показе через ITutorialItemSlotTargetProvider (см. его " +
                 "докстринг). Нужен, когда предмет может оказаться в любом слоте (например " +
                 "guidance-шаг 'найди и экипируй пистолет').")]
        public ItemId highlightItemId;
        public ItemId highlightInboxItemId;
        
        [Tooltip("Взаимоисключимо с highlightTargetId/highlightItemId/highlightInboxItemId. " +
                 "Highlight динамически найденного юнита в списке по критериям — см. " +
                 "TutorialUnitSearchCriteria.")]
        public TutorialUnitSearchCriteria highlightUnitSearch;

        [Tooltip("Взаимоисключимо с остальными highlight*-полями. Highlight карточки здания " +
                 "в панели строительства для этого facility item id — резолвится заново при " +
                 "каждом показе через ITutorialFacilitySlotTargetProvider (тот же 'живой " +
                 "поиск', что у highlightItemId, только по FacilityListView вместо инвентаря).")]
        public ItemId highlightFacilityItemId;

        [Tooltip("Взаимоисключимо с остальными highlight*-полями. Highlight кнопки вкладки " +
                 "этой категории в панели строительства — резолвится заново при каждом показе " +
                 "через ITutorialConstructionTabTargetProvider (кнопки вкладок пересоздаются " +
                 "целиком на каждый ConstructionPanelView.BuildTabs(), стабильного " +
                 "TutorialTargetId у них нет — тот же приём, что у highlightFacilityItemId). " +
                 "Nullable: enum-значение 0 (All) само по себе валидная категория, поэтому " +
                 "'не задано' нельзя выразить дефолтным значением enum, как для остальных " +
                 "reference-типовых highlight*-полей.")]
        public ConstructionCategory? highlightConstructionTabCategory;

        public TutorialTargetId arrowTargetId;

        [Header("Dialogue")]
        [Tooltip("Не обрабатывается — в проекте не найдена диалоговая система. Поле декларативно.")]
        public string dialogueId;

        [Header("Camera")]
        public TutorialTargetId cameraFocusTargetId;

        [Header("Input")]
        public TutorialInputMode inputPolicy = TutorialInputMode.Free;


        public bool HasVisuals =>
            !string.IsNullOrEmpty(instructionTitleKey) ||
            highlightTargetId != null ||
            highlightItemId != null ||
            highlightInboxItemId != null ||
            highlightUnitSearch != null ||
            highlightFacilityItemId != null ||
            highlightConstructionTabCategory.HasValue ||
            arrowTargetId != null ||
            !string.IsNullOrEmpty(dialogueId);
    }
    
    public enum HighlightMode
    {
        None,
        FixedTarget,        // highlightTargetId
        InventoryItem,       // highlightItemId
        InboxItem,           // highlightInboxItemId
        UnitSearch,          // highlightUnitSearch
        FacilityCard,        // highlightFacilityItemId
        ConstructionTab      // highlightConstructionTabCategory
    }
}
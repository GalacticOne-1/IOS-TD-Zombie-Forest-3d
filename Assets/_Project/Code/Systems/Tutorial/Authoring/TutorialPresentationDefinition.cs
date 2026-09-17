using System;
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
        
        [Header("Dialogue")]
        [Tooltip("Не обрабатывается — в проекте не найдена диалоговая система. Поле декларативно.")]
        public string dialogueId;


        [Header("Input")]
        public TutorialInputMode inputPolicy = TutorialInputMode.Free;


        public bool HasVisuals =>
            !string.IsNullOrEmpty(instructionTitleKey) ||
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
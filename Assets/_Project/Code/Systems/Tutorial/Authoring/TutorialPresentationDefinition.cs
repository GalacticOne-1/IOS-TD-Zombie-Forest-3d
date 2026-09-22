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
        
        [Header("Capabilities")]
        [Tooltip("Composable capability policy — раздел 10.1 ТЗ Chapter 2. Параллельно " +
                 "inputPolicy, для тонкой блокировки движения/камеры/interact/abilities " +
                 "независимо друг от друга (нужно для Molotov-шага: CanMove=false, " +
                 "CanControlCamera=false, CanInteract=false, CanUseAbilities=true — " +
                 "единый TutorialInputMode такую комбинацию не выражает). Не пересекается " +
                 "с InteractionPolicyService — тот остаётся политикой world-interactions.")]
        public bool canMove = true;
        public bool canControlCamera = true;
        public bool canInteract = true;
        public bool canUseAbilities = true;


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
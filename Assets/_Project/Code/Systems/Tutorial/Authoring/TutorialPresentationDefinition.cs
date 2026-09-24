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
                 "независимо друг от друга.")]
        public bool canMove = true;
        public bool canControlCamera = true;
        public bool canInteract = true;
        public bool canUseAbilities = true;

        [Header("Camera")]
        [Tooltip("Опциональное пространственное ограничение ручного движения камеры на " +
                 "этом шаге — независимо от canControlCamera (та означает полную блокировку, " +
                 "это — область при разрешённом управлении). Не guidance-условное — статично " +
                 "на уровне шага, см. TutorialService.BuildEffectivePresentation.")]
        public TutorialCameraConstraintDefinition cameraConstraint = new();


        public bool HasVisuals =>
            !string.IsNullOrEmpty(instructionTitleKey) ||
            !string.IsNullOrEmpty(dialogueId);

#if UNITY_EDITOR
        public bool Validate(out string error)
        {
            if (!cameraConstraint.Validate(out error))
                return false;

            error = null;
            return true;
        }
#endif
    }

    public enum HighlightMode
    {
        None,
        FixedTarget,
        InventoryItem,
        InboxItem,
        UnitSearch,
        FacilityCard,
        ConstructionTab
    }
}
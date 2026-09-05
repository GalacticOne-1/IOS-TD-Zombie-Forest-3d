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
        public string instructionTextKey;

        [Header("Highlight / Arrow")]
        public TutorialTargetId highlightTargetId;
        public TutorialTargetId arrowTargetId;

        [Header("Dialogue")]
        [Tooltip("Не обрабатывается — в проекте не найдена диалоговая система. Поле декларативно.")]
        public string dialogueId;

        [Header("Camera")]
        public TutorialTargetId cameraFocusTargetId;

        [Header("Input")]
        public TutorialInputMode inputPolicy = TutorialInputMode.Free;

        public bool HasVisuals =>
            !string.IsNullOrEmpty(instructionTextKey) ||
            highlightTargetId != null ||
            arrowTargetId != null ||
            !string.IsNullOrEmpty(dialogueId);
    }
}

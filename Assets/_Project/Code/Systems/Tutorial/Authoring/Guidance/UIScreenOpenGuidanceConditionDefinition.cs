using UnityEngine;
using Galactic1.UI.Core;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>Условие вида "экран (не) открыт". Требует ITutorialUIStateQuery.IsScreenOpen —
    /// см. её докстринг в Objectives/ITutorialUIStateQuery.cs про интеграционную точку в
    /// UIScreenManager, если подходящего метода там ещё нет (тот же паттерн "требует одну
    /// строку в X", что у уже существующих BLOCKED-объективов проекта).</summary>
    [CreateAssetMenu(fileName = "Guidance_UIScreenOpen",
        menuName = "Game Configs/Tutorial/Guidance Conditions/UI Screen Open")]
    public sealed class UIScreenOpenGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "UIScreenOpen";

        public UIScreenId screenId;

        [Tooltip("true = условие истинно, когда экран ОТКРЫТ; false = когда ЗАКРЫТ.")]
        public bool expectedOpen = true;

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (screenId == null)
            {
                error = "UIScreenOpenGuidanceConditionDefinition: screenId is empty.";
                return false;
            }
            error = null;
            return true;
        }
#endif
    }
}

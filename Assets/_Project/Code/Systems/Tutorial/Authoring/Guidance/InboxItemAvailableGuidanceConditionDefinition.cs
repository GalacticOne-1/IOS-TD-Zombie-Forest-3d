
using UnityEngine;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>"Предмет (не) лежит во входящих (Inbox)" — критерий выбора guidance-таргета
    /// на Inbox-карточку (highlightItemId у соответствующего guidance-варианта резолвится
    /// через CompositeTutorialItemSlotTargetProvider → TutorialInboxSlotTargetProvider).</summary>
    [CreateAssetMenu(fileName = "Guidance_InboxItemAvailable",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Inbox Item Available")]
    public sealed class InboxItemAvailableGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "InboxItemAvailable";

        public ItemId itemId;

        [Tooltip("true = условие истинно, когда предмет ЕСТЬ во входящих; false = когда его там нет.")]
        public bool expectedAvailable = true;
    }
}
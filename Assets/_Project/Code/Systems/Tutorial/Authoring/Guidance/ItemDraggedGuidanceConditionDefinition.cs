using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>"Предмет (не) перетаскивается сейчас" — читает DragManager.DraggedItemId
    /// (см. её докстринг про dragThreshold) через ITutorialInventoryInteractionQuery.
    ///
    /// KNOWN LIMITATION: как и у Item Selected — нет EventBus-события на конец драга
    /// (EndDrag() ничего не поднимает), переоценка триггерится следующим relevant-событием.
    /// Для типового кейса "довёл предмет до нужного слота" это не проблема — шаг обычно
    /// завершается через ItemEquippedObjective раньше, чем guidance успевает устареть
    /// (guidance останавливается вместе со степом, см. TutorialStepRuntimeState.Stop()).</summary>
    [CreateAssetMenu(fileName = "Guidance_ItemDragged",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Item Dragged")]
    public sealed class ItemDraggedGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "ItemDragged";

        [Tooltip("Пусто = засчитывается любой перетаскиваемый предмет.")]
        public ItemId itemId;

        [Tooltip("true = условие истинно, когда предмет ПЕРЕТАСКИВАЕТСЯ; false = когда нет.")]
        public bool expectedDragged = true;
    }
}
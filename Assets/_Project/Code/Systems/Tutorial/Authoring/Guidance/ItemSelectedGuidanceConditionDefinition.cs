using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>"Предмет (не) выбран кликом в инвентаре" — читает InventoryView.selectedSlot
    /// через ITutorialInventoryInteractionQuery, событие InventorySlotSelectEvent — только
    /// триггер переоценки.
    ///
    /// KNOWN LIMITATION: у InventoryView.ClearSelection() нет собственного EventBus-события —
    /// переоценка после снятия выбора произойдёт не мгновенно, а на следующее relevant-событие
    /// где-то ещё в guidance-списке. IsSatisfied() при этом не врёт (live-запрос), просто
    /// презентация может на мгновение отстать. Тот же класс ограничения, что у
    /// UIScreenOpenGuidanceCondition (см. её докстринг).</summary>
    [CreateAssetMenu(fileName = "Guidance_ItemSelected",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Item Selected")]
    public sealed class ItemSelectedGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "ItemSelected";

        [Tooltip("Пусто = засчитывается любой выбранный предмет.")]
        public ItemId itemId;

        [Tooltip("true = условие истинно, когда предмет ВЫБРАН; false = когда НЕ выбран.")]
        public bool expectedSelected = true;
    }
}
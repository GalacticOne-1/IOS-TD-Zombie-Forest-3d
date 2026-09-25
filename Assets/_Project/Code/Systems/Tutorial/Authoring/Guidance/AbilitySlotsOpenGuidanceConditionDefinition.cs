using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>Условие вида "список способностей юнита (не) открыт". Открытие фиксируется
    /// AbilitySlotsOpenedEvent (UnitCardPresenter.OnAbilityOpen) — только сигнал для
    /// переоценки, само значение не хранится (в отличие от UIScreenOpen, тут нет закрытия
    /// как отдельного события — карточка не различает "закрыт" vs "ещё не открывался").
    /// Обычно используется как expectedOpen=false, чтобы гасить highlight кнопки
    /// способностей сразу после первого открытия списка.</summary>
    [CreateAssetMenu(fileName = "Guidance_AbilitySlotsOpen",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Ability Slots Open")]
    public sealed class AbilitySlotsOpenGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "AbilitySlotsOpen";

        [Tooltip("true = условие истинно, когда список СЕЙЧАС ОТКРЫТ; false = когда ЗАКРЫТ " +
                 "(эта форма держит highlight кнопки, пока список закрыт, и гасит его при " +
                 "открытии — при повторном закрытии подсветка возвращается).")]
        public bool expectedOpen = true;
    }
}
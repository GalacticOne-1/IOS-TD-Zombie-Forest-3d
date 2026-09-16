using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Живой поиск "какая карточка здания сейчас показана в панели строительства" — тот
    /// же принцип, что ITutorialItemSlotTargetProvider (инвентарь/инбокс) и
    /// ITutorialUnitSlotTargetProvider (список юнитов): нет персистентной регистрации по
    /// TutorialTargetId, карточки пересоздаются при каждом Rebind() панели (см.
    /// FacilityListView.Build), поэтому ответ пересчитывается заново на каждый вызов.
    ///
    /// Если панель строительства сейчас не открыта (или карточка отфильтрована текущей
    /// вкладкой) — false, тот же валидный fallback "ничего не показывать", что и у
    /// остальных guidance-таргет-провайдеров.
    /// </summary>
    public interface ITutorialFacilitySlotTargetProvider
    {
        bool TryGetFacilityTarget(ItemId facilityItemId, out ITutorialTarget target);
    }
}

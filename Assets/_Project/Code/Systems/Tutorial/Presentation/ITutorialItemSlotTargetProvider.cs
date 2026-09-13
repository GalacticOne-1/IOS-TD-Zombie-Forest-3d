using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Живой резолв "какой UI-элемент сейчас показывает предмет X" — в отличие от
    /// TutorialTargetRegistry (register-once по фиксированному TutorialTargetId), здесь
    /// НЕТ персистентной регистрации по слоту: слот-виджет стабилен, но какой предмет он
    /// показывает — нет. Ответ пересчитывается заново на каждый вызов внутри
    /// TutorialPresentationService.Render (см. её докстринг про item-based highlight).
    ///
    /// Если предмета сейчас нигде не видно (ни один зарегистрированный InventoryView его
    /// не содержит — например, экран инвентаря ещё не открыт), возвращает false — тот же
    /// валидный fallback "ничего не показывать", что и у guidance в целом (см.
    /// TutorialGuidanceRuntimeState докстринг, п.19 исходного ТЗ).
    /// </summary>
    public interface ITutorialItemSlotTargetProvider
    {
        bool TryGetSlotTarget(ItemId itemId, out ITutorialTarget target);
    }
}

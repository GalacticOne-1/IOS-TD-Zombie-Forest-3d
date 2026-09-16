using Galactic1.Code.Systems.Construction.Configs;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Живой поиск "какая кнопка вкладки категории строительства сейчас на сцене" — тот
    /// же принцип, что ITutorialFacilitySlotTargetProvider: кнопки пересоздаются целиком
    /// на каждый ConstructionPanelView.BuildTabs() (Clear + Instantiate по списку категорий),
    /// у них нет стабильного TutorialTargetId, поэтому ответ пересчитывается заново на
    /// каждый вызов.
    ///
    /// Если панель строительства сейчас не открыта — false, тот же валидный fallback
    /// "ничего не показывать", что и у остальных guidance-таргет-провайдеров.
    /// </summary>
    public interface ITutorialConstructionTabTargetProvider
    {
        bool TryGetTabTarget(ConstructionCategory category, out ITutorialTarget target);
    }
}

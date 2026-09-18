using System;
using Galactic1.Code.Systems.Construction.Configs;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>Узкий query "какая вкладка категории сейчас выбрана в панели строительства" —
    /// по тому же принципу, что ITutorialFacilityPanelQuery (открыта ли панель вообще) и
    /// ITutorialConstructionQuery (построено ли здание): guidance condition получает только
    /// то, что ему нужно. Null = ни одна вкладка не выбрана (панель закрыта, либо открылась,
    /// но выбор ещё не сделан — если у панели вообще есть промежуточное "нет выбора"
    /// состояние; если категория выбирается автоматически при открытии, null практически
    /// не наблюдается).</summary>
    public interface ITutorialConstructionTabQuery
    {
        ConstructionCategory? CurrentTabCategory { get; }

        /// <summary>Триггер переоценки — тот же принцип, что OnFacilityBuilt/OnInboxChanged:
        /// событие не источник истины, CurrentTabCategory перечитывается заново.</summary>
        event Action<ConstructionCategory> OnConstructionTabSelected;
    }
}
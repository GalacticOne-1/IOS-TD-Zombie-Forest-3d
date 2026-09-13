using Galactic1.UI.Core;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>Узкий query-интерфейс "открыт ли сейчас экран X" — по тому же принципу,
    /// что ITutorialInventoryQuery/ITutorialSquadQuery (см. TutorialGameStateQuery
    /// докстринг): один класс резолвит через реальные UI-системы проекта, но guidance
    /// condition получает только то, что ему нужно.
    ///
    /// Требует одну интеграционную точку в UIScreenManager (метод "экран X сейчас
    /// открыт?"), если его там ещё нет — тот же паттерн, что у уже существующих
    /// "требует одну строку в Y" объективов (см. UIScreenOpenedObjective, который уже
    /// требует событие открытия из того же UIScreenManager).</summary>
    public interface ITutorialUIStateQuery
    {
        bool IsScreenOpen(UIScreenId screenId);
    }
}

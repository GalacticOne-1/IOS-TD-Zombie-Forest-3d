using System;
using Galactic1.Code.Inventory.Context;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>"Какая главная вкладка панели инвентаря сейчас выбрана" — узкий query по
    /// тому же принципу, что ITutorialFacilityPanelQuery/ITutorialConstructionQuery. Null =
    /// ни одна вкладка этого блока ещё не выбиралась в этой сессии окна.</summary>
    public interface ITutorialInventoryMainTabQuery
    {
        InventoryGameplayMode? CurrentMainTab { get; }
        event Action<InventoryGameplayMode> OnMainTabSelected;
    }
}
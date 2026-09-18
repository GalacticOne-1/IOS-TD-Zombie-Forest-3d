using System;
using Galactic1.Code.Inventory.Context;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public interface ITutorialInventorySquadExtraTabQuery
    {
        InventoryGameplayMode? CurrentSquadExtraTab { get; }
        event Action<InventoryGameplayMode> OnSquadExtraTabSelected;
    }
}
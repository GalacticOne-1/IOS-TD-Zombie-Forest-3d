using System;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>"Есть ли сейчас на сцене ghost этого здания" — узкий query по тому же
    /// принципу, что ITutorialFacilityPanelQuery/ITutorialConstructionQuery. Null =
    /// ghost сейчас нет вообще (независимо от того, какое здание запрашивали).</summary>
    public interface ITutorialConstructionGhostQuery
    {
        ItemId CurrentGhostFacilityItemId { get; }
        event Action<ItemId> OnGhostChanged;
    }
}
using UnityEngine;

namespace Galactic1.Gameplay.Interaction
{
    public enum ActionType
    {
        None,
        OpenContainer,
        LootPickup,
        GatherTree,
        GatherOre,
        
    }
    public interface IActionInteractable
    {
        ActionType ActionType { get; }
    }
}
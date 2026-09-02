using UnityEngine;

namespace Galactic1.Code.Gameplay.Interaction
{
    /// <summary>
    /// Объект, с которым может взаимодействовать игрок.
    /// </summary>
    public interface IInteractable
    {
        GameObject GetObject { get; }
        void OnInteract();
    }
}
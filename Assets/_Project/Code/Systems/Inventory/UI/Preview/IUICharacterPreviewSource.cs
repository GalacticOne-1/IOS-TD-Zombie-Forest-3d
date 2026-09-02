using UnityEngine;

namespace Galactic1.UI.Inventory.Preview
{
    /// <summary>
    /// Источник данных для предпросмотра:
    /// игрок, дракон, NPC и т.д.
    /// </summary>
    public interface IUICharacterPreviewSource
    {
        GameObject Prefab { get; }

    }
}
using System;
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    [Serializable]
    public class UIButtonVisualRule
    {
        public GameObject button; // чтобы включать / выключать interactable
        public string styleId;

        public Func<InventoryManagementWindow, bool> isEnabled; // можно ли нажать
        public Func<InventoryManagementWindow, bool> isHighlighted; // нужно ли подсветить зелёным
    }
}
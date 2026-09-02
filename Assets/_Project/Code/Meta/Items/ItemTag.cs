using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    [CreateAssetMenu(menuName = "Game Configs/Inventory/Item Tag")]
    public class ItemTag : ScriptableObject
    {
        public string tagId; // example: "food", "medical", "military"
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.UI.Core
{
    [CreateAssetMenu(fileName = "UIRegistry", menuName = "Game Configs/UI/UI Registry")]
    public class UIRegistry : ScriptableObject
    {
        [System.Serializable]
        public class UIEntry
        {
            public UIScreenId id;       // "InventoryWindow"
            public string path;         // "UI/Windows/InventoryWindow"
        }

        public List<UIEntry> entries = new();

        private Dictionary<UIScreenId, string> lookup;

        public string GetPath(UIScreenId id)
        {
            lookup ??= BuildLookup();
            return lookup.TryGetValue(id, out var path) ? path : null;
        }

        private Dictionary<UIScreenId, string> BuildLookup()
        {
            var dict = new Dictionary<UIScreenId, string>();
            foreach (var e in entries) dict[e.id] = e.path;
            return dict;
        }
    }

}
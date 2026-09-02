
using System;
using System.Collections.Generic;
using Galactic1.Game.UI.Stats;
using UnityEngine;

namespace Galactic1.UI.Core
{
    [CreateAssetMenu(
        fileName = "StatLayoutConfig", 
        menuName = "Game Configs/Style/Stat Layout Config")]
    public class StatLayoutConfig : StyleConfigBase
    {
        [Serializable]
        private struct Entry
        {
            public StatLayoutType Type;
            public MonoBehaviour Prefab;
        }

        [SerializeField] private List<Entry> entries;
        [SerializeField] private Sprite[] iconCompare;
        
        

        private Dictionary<StatLayoutType, MonoBehaviour> _cache;

        public void TryGet(StatLayoutType type, out MonoBehaviour prefab)
        {
            if (_cache == null)
            {
                _cache = new Dictionary<StatLayoutType, MonoBehaviour>();
                foreach (var e in entries)
                    _cache[e.Type] = e.Prefab;
            }

            prefab = _cache[type];
        }


        public Sprite GetCompareIcon(TooltipDataFieldStyle style)
        {
            if (style == TooltipDataFieldStyle.Green)
                return iconCompare[0];
            
            if (style == TooltipDataFieldStyle.Red)
                return iconCompare[1];

            return null;
        }
    }
}
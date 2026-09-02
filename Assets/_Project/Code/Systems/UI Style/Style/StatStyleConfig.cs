using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Game.UI.Stats;
using TMPro;
using UnityEngine;

namespace Galactic1.UI.Core
{

    /// <summary>
    /// ScriptableObject-конфиг, содержащий стили отображения статов.
    /// </summary>
    [CreateAssetMenu(
        fileName = "StatStyleConfig",
        menuName = "Game Configs/Style/Stat Style Config")]
    public class StatStyleConfig : ScriptableObject
    {
        [SerializeField] private List<StatStyleEntry> entries = new();
        [SerializeField] private List<DescriptorStyleEntry> descriptorEntries = new();

        
        private Dictionary<StatId, StatStyleEntry> _lookup;
        private Dictionary<DescriptorId, DescriptorStyleEntry> _lookupDescriptor;

        public void Initialize()
        {
            // #1
            _lookup = new ();

            foreach (var entry in entries)
            {
                if (!_lookup.ContainsKey(entry.statId))
                    _lookup.Add(entry.statId, entry);
            }
            
            
            // #2
            _lookupDescriptor = new ();

            foreach (var entry in descriptorEntries)
            {
                if (!_lookupDescriptor.ContainsKey(entry.descriptorId))
                    _lookupDescriptor.Add(entry.descriptorId, entry);
            }
        }

        public StatStyleEntry GetStat(StatId statId)
        {
            return _lookup.TryGetValue(statId, out var entry)
                ? entry
                : null;
        }
        
        public DescriptorStyleEntry GetDescriptor(DescriptorId descriptorId)
        {
            return _lookupDescriptor.TryGetValue(descriptorId, out var entry)
                ? entry
                : null;
        }
    }


    /// <summary>
    /// Описывает правила отображения одного StatId.
    /// </summary>
    [System.Serializable]
    public class StatStyleEntry
    {
        [Header("Text")] 
        public string localizationKey;
        
        [Tooltip("добавить перед значением")]
        public string front;
        [Tooltip("добавить после значения")]
        public string suffix;
        [Tooltip("добавить в конец localizationKey >> :")]
        public bool appendColon;
        [Tooltip("отображать поле в формате >> 30/100")]
        public bool currentMaxField;
        
        [Header("Key")] 
        public StatId statId;

        [Header("Layout")] 
        public StatLayoutType layoutType;
        

        [Header("Visual")] 
        public Color valueColor = Color.white;
        public TMP_FontAsset font;
        public Sprite icon;

        [Header("Formatting")] 
        public bool showPlusForPositive;
        public bool roundToInt;
        public int decimalPlaces = 1;
    }
    
    
    [System.Serializable]
    public class DescriptorStyleEntry 
    {
        public DescriptorId descriptorId;
        
        [Header("Layout")]
        public StatLayoutType layoutType;
        
        [Header("Либо staticLayoutId либо statId")]
        [Tooltip("Ключ для статичного поля")]
        public string staticLayoutId;
        //[Tooltip("Для префаба")]
        //public StatId statId;
        

        [Header("Visual")]
        public Color textColor = Color.white;
        public TMP_FontAsset font;
        public Sprite icon;
    }
}
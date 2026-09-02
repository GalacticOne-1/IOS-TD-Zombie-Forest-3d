using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Galactic1.Code.UI.Tooltips
{
    /// <summary>
    /// Централизованный контроллер подсказок.
    /// Создаёт все tooltip'ы при старте и управляет их отображением.
    /// </summary>
    public class TooltipController : MonoBehaviour, IGameService
    {
        [Serializable]
        private struct TooltipEntry
        {
            public TooltipType type;
            public TooltipUI prefab;
        }

        [Header("Tooltip Prefabs")]
        [SerializeField] private TooltipEntry[] tooltipPrefabs;

        [SerializeField] private Transform root;
        [SerializeField] private float holdThreshold = .5f;

        private readonly Dictionary<TooltipType, TooltipUI> tooltips = new();
        private TooltipUI activeTooltip;

        
        
        
        
        
        
        private void Awake()
        {
            CreateTooltips();
            HideAll();
        }

        private void CreateTooltips()
        {
            foreach (var entry in tooltipPrefabs)
            {
                var instance = Instantiate(entry.prefab, root);
                instance.gameObject.SetActive(true);
                tooltips.Add(entry.type, instance);
            }
        }

        private void HideAll()
        {
            foreach (var tooltip in tooltips.Values)
                tooltip.Hide();

            activeTooltip = null;
        }

        /// <summary>
        /// Открывает tooltip заданного типа.
        /// </summary>
        public T Show<T>(
            TooltipType type,
            RectTransform anchor,
            object data,
            int durability,
            HintSource hintSource = HintSource.Default,
            Action<T> bindAction = null
        ) where T : TooltipUI
        {
            HideAll();

            if (!tooltips.TryGetValue(type, out var tooltip))
            {
                Debug.LogError($"Tooltip of type {type} not registered");
                return null;
            }

            
            tooltip.Launch(anchor, holdThreshold, HintResolver.Formatting(hintSource, data, durability));

            var typedTooltip = tooltip as T;
            bindAction?.Invoke(typedTooltip);

            activeTooltip = tooltip;
            return typedTooltip;
        }

        public void Hide()
        {
            if (activeTooltip == null)
                return;

            activeTooltip.Hide();
            activeTooltip = null;
        }
    }
    
    public enum TooltipType
    {
        Loot,
        Survivor
    }
}

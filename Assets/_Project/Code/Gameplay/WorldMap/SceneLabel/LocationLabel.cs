
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.WorldMap
{
    /// <summary>
    /// Мини-панель над локацией на карте: название + иконка типа локации.
    /// Создаётся и управляется WorldMapController / NodeManager.
    /// </summary>
    public class LocationLabel : WorldMapLabelBase
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image bgImage;
        

        /// <summary>
        /// Привязать маркер к конкретной ноде.
        /// </summary>
        public override void Bind(MapNode node)
        {
            base.Bind(node);
            
            UpdateMarker();

            // Подписка на изменения состояния ноды
            node.OnNodeStateChanged += OnNodeStateChanged;
        }

        // private void OnDestroy()
        // {
        //     if (boundNode != null)
        //         boundNode.OnNodeStateChanged -= OnNodeStateChanged;
        // }
        
        
        
        

        /// <summary>
        /// Обновление текста и иконки
        /// </summary>
        public void UpdateMarker()
        {
            if (boundNode == null) return;

            var style = ServiceLocator.Current.Get<UIStyleResolver>()
                .GetLocationStyle(boundNode.Config.LocationType, boundNode.Config.Difficulty);
            
            titleText.text = boundNode.Config.Header.TitleLid;
            iconImage.sprite = style.Item1;
            iconImage.color = style.Item2;
            bgImage.color = style.Item2;
        }

        private void OnNodeStateChanged(MapNode node)
        {
            UpdateMarker();
        }
    }
}
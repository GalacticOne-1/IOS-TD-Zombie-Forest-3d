using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Galactic1.Game.UI.Production
{
    /// <summary>
    /// Одна карточка рецепта.
    /// Отвечает только за визуал и события клика.
    /// </summary>
    public sealed class RecipeCardView : ButtonUIProgrammatic
    {
        [Header("UI")]
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text title;

        [Header("States")]
        [SerializeField] private GameObject selected;
        [SerializeField] private GameObject locked;

        /// <summary>
        /// Для доступа к рецепту в конфиге ItemBase 
        /// </summary>
        public RuntimeId RecipeId { get; private set; }

        public event Action<RuntimeId> OnClicked;

        
        
        
        public override void Initialize(DIContainer container = null)
        {
            gameObject.RegisterButtonClick(HandleClick);
        }
        
        private void OnDestroy()
        {
            
        }

        public void Setup(RecipeCardData data)
        {
            RecipeId = data.RecipeId;

            
            title.text = data.Name;
            
            icon.material = ServiceLocator.Current.Get<UIStyleResolver>().ResolveRarityColor(data.Rarity).Material;
            icon.sprite = data.Icon;

            locked.SetActive(!data.IsAvailable);
            //gameObject.ButtonSetInteractable(data.IsAvailable);

            SetSelected(false);
        }

        public void SetSelected(bool value)
        {
            selected.SetActive(value);
        }

        void HandleClick()
        {
            OnClicked?.Invoke(RecipeId);
        }

        public void Click() => HandleClick();

    }
}
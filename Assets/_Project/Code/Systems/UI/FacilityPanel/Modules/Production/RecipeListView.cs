using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Game.UI.Production
{
    /// <summary>
    /// Список доступных рецептов (ScrollView).
    /// Отвечает только за создание и управление карточками.
    /// Бизнес-логики здесь нет.
    /// </summary>
    public sealed class RecipeListView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private RecipeCardView cardPrefab;

        private readonly List<RecipeCardView> cards = new();

        public event Action<RuntimeId> OnRecipeSelected;

        /// <summary>
        /// Создание списка карточек.
        /// </summary>
        public void Build(List<RecipeCardData> recipes)
        {
            Clear();

            foreach (var data in recipes)
            {
                var card = Instantiate(cardPrefab, contentRoot);
                card.Setup(data);
                card.OnClicked += HandleCardClicked;

                cards.Add(card);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot as RectTransform);
            scrollRect.SetSizeContentGridLayoutGroup(true, false, true, true);
        }

        /// <summary>
        /// Выделить выбранную карточку.
        /// </summary>
        public void SetSelected(RuntimeId itemId)
        {
            foreach (var card in cards)
            {
                card.SetSelected(card.RecipeId == itemId);
            }
        }

        private void HandleCardClicked(RuntimeId recipeId)
        {
            SetSelected(recipeId);
            OnRecipeSelected?.Invoke(recipeId);
        }
        
        public void SelectCard(RuntimeId recipeId)
        {
            foreach (var card in cards)
                if (card.RecipeId == recipeId)
                {
                    card.Click();
                    break;
                }
        }

        private void Clear()
        {
            foreach (var card in cards)
            {
                card.OnClicked -= HandleCardClicked;
                card.gameObject.SetActive(false);
                Destroy(card.gameObject);
            }

            cards.Clear();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.Economy;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;

namespace Galactic1.Code.UI.Construction
{
    /// <summary>
    /// UI карточка здания в списке строительства.
    /// Показывает:
    /// - Основную иконку здания с overlay (можно строить / нельзя)
    /// - Название
    /// - Кнопку build
    /// - Слоты ингредиентов рецепта
    /// </summary>
    public class ConstructionFacilityCardView : ButtonUIProgrammatic
    {
        [Header("Main UI")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject mainSlot;

        [Header("Recipe")]
        [SerializeField] private Transform recipesRoot;

        [Header("Limit")]
        [SerializeField] private GameObject dimOverlay;
        [SerializeField] private GameObject limitLabel;


        private UIStyleResolver _styleResolver;
        private ConstructionRequirementService _requirementService;
        private FacilityModule _facility;
        private Action<FacilityModule> _onSelected;

        /// <summary>
        /// Инициализация карточки
        /// </summary>
        public void Bind(
            ConstructionRequirementService requirementService,
            FacilityModule facility,
            UIStyleResolver styleResolver,
            Action<FacilityModule> onSelected,
            bool limitReached = false)
        {
            _requirementService = requirementService;
            _styleResolver = styleResolver;
            _facility = facility;
            _onSelected = onSelected;

            titleText.text = facility.Item.Header.titleLid;
            mainSlot.GetChild(0, 1).CMP_Image().sprite = facility.Item.Header.icon;
            // ServiceLocator.Current.Get<PreviewService>().RequestSprite(
            //     facility.Item.PrefabName,
            //     null,
            //     sprite => mainSlot.GetChild(0, 1).CMP_Image().sprite = sprite
            // );

            gameObject.RegisterButtonClick(OnClick);
            
            // Затемнение / блокировка если лимит исчерпан
            dimOverlay.SetActive(limitReached);
            limitLabel.SetActive(limitReached); 
            SetInteractable(!limitReached);

            UpdateView(limitReached);
        }

        /// <summary>
        /// Полное обновление карточки
        /// </summary>
        public void UpdateView(bool limitReached)
        {
            UpdateMainOverlay(limitReached);
            UpdateRecipe(limitReached);
        }

        /// <summary>
        /// Обновление overlay здания
        /// </summary>
        private void UpdateMainOverlay(bool limitReached)
        {
            bool canBuild = _requirementService.CanBuild(_facility);
            mainSlot.GetChild(0, 0).gameObject.SetActive(!limitReached && canBuild);
        }

        /// <summary>
        /// Обновление рецепта
        /// </summary>
        private void UpdateRecipe(bool limitReached)
        {
            CraftRecipeConfig recipe;
            List<RequirementData> requirement;
            if (_facility.Item.Recipes == null || _facility.Item.Recipes.Count == 0)
            {
                recipe = new();
                requirement = new(0);
            }
            else
            {
                recipe = _facility.Item.Recipes[0];
                requirement = recipe.Requirement.ToList();
            }
            
            var l = recipesRoot.childCount;
            for (int i = 0; i < l; i++)
            {
                var slotView = recipesRoot.GetChild(i).GetComponent<ConstructionRequiresSlotView>();
                if (i < requirement.Count)
                {
                    var req = requirement[i];
                    bool hasEnough = _requirementService.HasResources(req);

                    // === при достигнутом лимите не карточка не активна
                    if (limitReached) hasEnough = false;
                    
                    if (req.Item == null)
                    {
                        Debug.LogError($"Recipe error => {_facility.Item.Header.titleLid}");
                        continue;
                    }

                    slotView.Bind(
                        _styleResolver,
                        _requirementService.GetOwnedAmount(req),
                        req,
                        hasEnough);
                }
                else
                {
                    slotView.Hide();
                }
            }
        }


        private void OnClick()
        {
            _onSelected?.Invoke(_facility);
        }
    }
}
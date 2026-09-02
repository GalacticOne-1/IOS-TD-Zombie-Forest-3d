using System;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Sources;
using Galactic1.Code.Systems.Economy;
using Galactic1.Configs;
using Galactic1.Core.Systems.GameLoopSession;
using UnityEngine;
using Galactic1.Game.Runtime.Production;
using Galactic1.Game.UI.Production.DTO;
using Galactic1.Items;
using UnityEngine.UI;

namespace Galactic1.Game.UI.Production
{
    /// <summary>
    /// Отображает требования рецепта и кнопку Add Order.
    /// Детальной инфо здесь нет.
    /// </summary>
    public sealed class RecipeDetailsView : MonoBehaviour
    {
        [SerializeField] private ScrollRect statScroll;
        
        [Header("Requirements")]
        [SerializeField] private ScrollRect requiresItemScroll;
        [SerializeField] private RecipeRequireSlotView requireViewPrefab;

        [Header("Actions")]
        [SerializeField] private GameObject addOrderButton;

        [Header("Alerts")] 
        [SerializeField] private GameObject stationUpgradeAlert;
        [SerializeField] private GameObject alertBlueprintAlert;


        public ScrollRect StatScroll => statScroll;



        private RecipeRequireSlotView[] requireSlots;
        
        bool isInitialized;
        
        public RecipeDetailsDto RecipeDto {get; private set;}
        public event Action<RuntimeId, ProcessingMode> OnAddOrderClicked;

        
        
        private void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;
            
            addOrderButton.RegisterButtonClick(AddOrder);

            // local pool
            requireSlots = new RecipeRequireSlotView[10];
            for (int i = 0; i < 10; i++)
            {
                requireSlots[i] = requireViewPrefab.gameObject
                    .CreateGO(requiresItemScroll.content).GetComponent<RecipeRequireSlotView>();
            }
        }

        public void ShowDetails(RecipeDetailsDto dto)
        {
            Initialize();
            
            RecipeDto = dto;
            
            // === Requirements
            LoadRequirements();

            
            // === Button state & alerts
            addOrderButton.ButtonSetInteractable(dto.CanAddOrder);
            addOrderButton.SetActive(dto.OrderButtonActive);
            stationUpgradeAlert.SetActive(dto.StationRequiresCtx.requiresStationUpgrade);
            stationUpgradeAlert.GetChild(0).CMP_Text().text = dto.StationRequiresCtx.stationAlertMessage;
            alertBlueprintAlert.SetActive(dto.StationRequiresCtx.requiresBlueprint);
        }

        private void AddOrder()
        {
            OnAddOrderClicked?.Invoke(
                RecipeDto.RecipeId,
                ProcessingMode.Standard
            );
        }
        

        private void LoadRequirements()
        {
            var requirementsCount = RecipeDto.Requirements.Count;
            var l = requireSlots.Length;
            for (int i = 0; i < l; i++)
            {
                if (i >= requirementsCount)
                {
                    requireSlots[i].gameObject.SetActive(false);
                    continue;
                }

                requireSlots[i].gameObject.SetActive(true);
                var requireSlot = requireSlots[i];
                var requirement = RecipeDto.Requirements[i];

                requireSlot.Setup(
                    requirement.ItemId,
                    requirement.Item,
                    requirement.Icon,
                    requirement.OwnedAmount,
                    requirement.RequiredAmount,
                    requirement.IsEnough
                );
            }

            if (requirementsCount > l)
            {
                Debug.LogError($"Recipe requirement more slots -> {RecipeDto.Title}");
            }

            requiresItemScroll.SetSizeContentLayoutGroup(false, null, true, true);
            requiresItemScroll.ScrollRectResetH(0);

        }

    }
}
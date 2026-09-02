
using System;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.UI.Buildings;
using Galactic1.Game.Runtime.Production;
using UnityEngine;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.Game.UI.Production.Presenters;
using Galactic1.UI.Core;
using UnityEngine.UI;

namespace Galactic1.Game.UI.Production
{
    /// <summary>
    /// Главная панель производства.
    /// Содержит три блока:
    /// - очередь
    /// - список рецептов
    /// - детали рецепта
    /// </summary>
    public sealed class ProductionPanelModule : FacilityPanelModule
    {
        [Header("Blocks")] 
        [SerializeField] private ProductionQueueView queueView;
        [SerializeField] private RecipeListView recipeListView;
        [SerializeField] private RecipeDetailsView detailsView;
        [SerializeField] private CraftDetailsView craftDetailsView;
        [SerializeField] private RecyclerDetailsView recyclerDetailsView;

        private IRecipeDetailsPresenter _detailsPresenter;
        private UniversalProductionSceneAdapter _adapter;
        private bool reset;

        
        
        public override bool IsSupported(FacilityDTO dto)
            => dto.Details.Type == FacilityType.Production ||
               dto.Details.Type == FacilityType.Recycler;

        
        public override void Bind(
            FacilityDTO dto, 
            object sceneAdapter,
            FacilityUpgradeSceneAdapter upgradeAdapter)
        {
            base.Bind(dto, sceneAdapter, upgradeAdapter);
            _adapter = sceneAdapter as UniversalProductionSceneAdapter;

            var details = dto.Details as ProductionFacilityDetailsDTO;

            // === Presenter selection
            if (details.Type == FacilityType.Recycler)
            {
                _detailsPresenter = new RecyclerRecipeDetailsPresenter(recyclerDetailsView);
                craftDetailsView.gameObject.SetActive(false);
            }
            else
            {
                _detailsPresenter = new CraftRecipeDetailsPresenter(craftDetailsView);
                recyclerDetailsView.gameObject.SetActive(false);
            }

            BindEvents();
            Rebind(dto);
            ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait1(() =>
            {
                // Форсим layout
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(craftDetailsView.StatRoot);
            });
        }

        private void BindEvents()
        {
            queueView.OnTakeClicked += HandleTake;
            queueView.OnSkipClicked += HandleSkip;
            recipeListView.OnRecipeSelected += HandleRecipeSelected;
            detailsView.OnAddOrderClicked += HandleAddOrder;
            queueView.OnSlotClicked += HandleSlotClicked;
            _adapter.OnCollectFailed += HandleCollectFailed;
        }

        public override void Unbind()
        {
            base.Unbind();
            if(_adapter != null)
            {
                queueView.OnTakeClicked -= HandleTake;
                queueView.OnSkipClicked -= HandleSkip;
                recipeListView.OnRecipeSelected -= HandleRecipeSelected;
                detailsView.OnAddOrderClicked -= HandleAddOrder;
                queueView.OnSlotClicked -= HandleSlotClicked;
                _adapter.OnCollectFailed -= HandleCollectFailed;
                
                _detailsPresenter.Clear();
            }
        }

        public override void Rebind(FacilityDTO dto)
        {
            var details = dto.Details as ProductionFacilityDetailsDTO;

            queueView.Bind(dto);
            recipeListView.Build(details.Recipes);
            
            // возвращаем выбор рецепта
            if (GameContent.Items.TryGet(_adapter.CurrentRecipe, out var item))
            {
                recipeListView.SelectCard(_adapter.CurrentRecipe);
            }
            else // === Автовыбор первого рецепта
            {
                if (details.Recipes != null && details.Recipes.Count > 0)
                {
                    recipeListView.SelectCard(details.Recipes[0].RecipeId);
                }
            }
        }

        private void HandleRecipeSelected(RuntimeId recipeId)
        {
            _adapter.SetCurrentRecipe(recipeId);
            var recipeDto = _adapter.GetRecipeDetails(recipeId);
            _detailsPresenter.Show(recipeDto);
            
        }
        
        private void HandleAddOrder(RuntimeId recipeId, ProcessingMode mode)
        {
            if (_adapter.TryAddOrder(recipeId))
            {
                // обновление UI произойдёт через OnStateChanged → Refresh
            }
        }
        
        private void HandleSlotClicked(string jobId, ProductionJobState state)
        {
            var job = _adapter
                .GetQueue()
                .FirstOrDefault(j => j.JobId == jobId);

            if (job == null)
                return;

            // A. если есть готовые заказы — забираем по одному
            if (job.CompletedStack > 0)
            {
                _adapter.CollectSingle(jobId);
                return;
            }

            // B. если готовых нет — отменяем заказ
            _adapter.CancelOrder(jobId);
        }

        private void HandleTake() => _adapter.CollectCompleted();

        private void HandleSkip() => _adapter.TryPaidSkip();

        /// <summary>
        /// Оповещение игрока о нехватке места в инвентаре
        /// </summary>
        private void HandleCollectFailed()
        {
            ServiceLocator.Current.Get<UIManager>().OpenPopup(
                UIScreenId.AdAlertToast,
                "Not enough space in storage!");
        }


    }
}
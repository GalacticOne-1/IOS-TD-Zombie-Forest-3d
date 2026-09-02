using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Core;
using Galactic1.Systems.Purchase;
using Galactic1.UI.Shop.Rewards;
using UnityEngine;

namespace Galactic1.UI.Shop
{
    /// <summary>
    /// Центральный контроллер магазина.
    /// Управляет состояниями, каталогом и покупками через FSM.
    /// </summary>
    public class ShopController : MonoBehaviour
    {

        //[Header("UI")]
        //[SerializeField] private ShopScrollView scrollView;


        private DIContainer _rootContainer;
        private GameStoreService _gameStoreService;
        private PurchaseService _purchaseService;
        private ShopRewardService rewardService;
        private ShopStateMachine fsm;
        private readonly HashSet<string> purchasedItems = new();


        private ShopCardUIBase currentView;
        public event Action OnShopOpened;
        public event Action OnShopClosed;
        public event Action<ShopState> OnStateChanged;
        public Action OnPurchaseSuccess;

        
        
        
        
        public void Initialize(
            DIContainer container, 
            GameStoreService gameStoreService,
            PurchaseService purchaseService)
        {
            _rootContainer = container;
            _gameStoreService = gameStoreService;
            _purchaseService = purchaseService;
            rewardService = new ShopRewardService(container);


            fsm = new ShopStateMachine();
            fsm.OnStateChanged += HandleStateChanged;

            _purchaseService.OnRestoreStarted += OnRestoreStarted;
            _purchaseService.OnRestoreCompleted += OnRestoreCompleted;
            _purchaseService.OnRestoreFailed += OnRestoreFailed;

            _purchaseService._OnPurchaseSuccess += OnPremiumPurchaseSuccess;
            _purchaseService._OnPurchaseFailed += OnPremiumPurchaseFailed;

            LoadPurchasedState();
        }

        private void OnDestroy()
        {
            if (_purchaseService == null) return;

            _purchaseService.OnRestoreStarted -= OnRestoreStarted;
            _purchaseService.OnRestoreCompleted -= OnRestoreCompleted;
            _purchaseService.OnRestoreFailed -= OnRestoreFailed;

            _purchaseService._OnPurchaseSuccess -= OnPremiumPurchaseSuccess;
            _purchaseService._OnPurchaseFailed -= OnPremiumPurchaseFailed;
        }

        #region FSM Handler

        private void HandleStateChanged(ShopState state)
        {
            OnStateChanged?.Invoke(state);

            // Управление UI блокировкой через FSM
            switch (state)
            {
                case ShopState.Purchasing:
                case ShopState.Restoring:
                    _rootContainer.Resolve<UIRootView>().ShowPurchaseScreen();
                    break;
                case ShopState.Ready:
                case ShopState.Closed:
                    _rootContainer.Resolve<UIRootView>().HidePurchaseScreen();
                    currentView = null;
                    break;
            }
        }

        #endregion

        #region Lifecycle

        public void Open()
        {
            fsm.TransitionTo(ShopState.Ready);
            
            //scrollView.Build(_rootContainer.Resolve<ShopInitializer>().ShopItems, OnBuyClicked);
            OnShopOpened?.Invoke();
            
            //transform.GetChild(0).gameObject.SetActive(true);
        }

        public void Close()
        {
            OnShopClosed?.Invoke();
            fsm.TransitionTo(ShopState.Closed);
            //transform.GetChild(0).gameObject.SetActive(false);
        }

        #endregion

        #region Restore

        public void OnRestoreClicked()
        {
            if (!fsm.CanTransitionTo(ShopState.Restoring)) return;

            _purchaseService.RestorePurchases();
        }

        private void OnRestoreStarted()
        {
            fsm.TransitionTo(ShopState.Restoring);
        }

        private void OnRestoreCompleted()
        {
            EndPurchaseFlow(success: true);
        }

        private void OnRestoreFailed(PurchaseResult result)
        {
            EndPurchaseFlow(success: false);
        }

        #endregion

        #region Purchase

        public void OnBuyClicked(IAPConfig config, ShopCardUIBase view)
        {
            if (!fsm.CanTransitionTo(ShopState.Purchasing)) return;
            
            //var iapConfig = viewModel._iapConfig as IAPConfig;
            
            if (config.OneTimePurchase && purchasedItems.Contains(config.ProductId)) return;
            
            var request = ShopPurchaseRequestFactory.Create(config, _rootContainer);
            if (!request.CanPurchase(config))   // покупка не возможна
            {
                //W_Options.I.Vibro();
                return;
            }

            currentView = view;
            
            // Блокируем UI только если это IAP или внешний запрос
            bool shouldBlockUI = config.UseIAP;

            if (shouldBlockUI)
                fsm.TransitionTo(ShopState.Purchasing);
            
            request.RequestPurchase(
                _purchaseService,
                config,
                onSuccess: () =>
                {
                    
                },
                onFail: () =>
                {
                    
                });
        }

        /// <summary>
        /// Награда для платной покупки
        /// </summary>
        /// <param name="productId"></param>
        void OnPremiumPurchaseSuccess(string productId)
        {
            var config = _gameStoreService.IapConfigsMap
                .First(c => c.Value.ProductId == productId).Value;
            if (config == null || config is not IAPConfig iap) 
                return;

            Grant(iap);
            
            
            if (fsm.CanTransitionTo(ShopState.Ready))
                EndPurchaseFlow(success: true);
        }

        void OnPremiumPurchaseFailed(string productId, PurchaseResult result)
        {
            if (fsm.CanTransitionTo(ShopState.Ready))
                EndPurchaseFlow(success: false);
        }

        /// <summary>
        /// Награда для покупки за хард
        /// </summary>
        /// <param name="config"></param>
        public void CompleteHardPurchase(IAPConfig config)
        {
            Grant(config);
            if (fsm.CanTransitionTo(ShopState.Ready))
                EndPurchaseFlow(success: true);
        }

        void EndPurchaseFlow(bool success)
        {
            if (fsm.Current == ShopState.Purchasing || fsm.Current == ShopState.Restoring)
                fsm.TransitionTo(ShopState.Ready);
        }


        #endregion

        #region Grant / Save

        private void Grant(IAPConfig config)
        {
            if (config.OneTimePurchase && purchasedItems.Contains(config.ProductId)) return;

            OnPurchaseSuccess?.Invoke();
            rewardService.Grant(config, currentView);

            if (config.OneTimePurchase)
            {
                purchasedItems.Add(config.ProductId);
                SavePurchasedState();
            }

            _rootContainer.Resolve<IGameStateProvider>().SaveGameState();
        }

        private void LoadPurchasedState()
        {
            var raw = PlayerPrefs.GetString("SHOP_PURCHASED", string.Empty);
            foreach (var id in raw.Split(';'))
            {
                if (!string.IsNullOrEmpty(id))
                    purchasedItems.Add(id);
            }
        }

        private void SavePurchasedState()
        {
            PlayerPrefs.SetString("SHOP_PURCHASED", string.Join(";", purchasedItems));
        }

        #endregion
    }
}

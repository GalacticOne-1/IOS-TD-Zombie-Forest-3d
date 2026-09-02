using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Systems.Purchase;
using Galactic1.UI.Core;
using ObservableCollections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Galactic1.UI.Shop
{
    public class GameStoreService : ISdk
    {
        private readonly DIContainer _rootContainer;
        public ESdkType SdkType => ESdkType.Iap;
        
        
        public ShopController _shopController { get; private set; }
        public PurchaseService _purchaseService { get; private set; }
        
        private GameStoreWindow _gameStoreWindow;
        
        public readonly IObservableCollection<ShopCardProxy> _cardProxies;
        
        
        public readonly Dictionary<string, IAPConfig> IapConfigsMap = new();
        private Dictionary<int, ShopCardViewModel> _createdCardsMap = new();
        
        private ObservableList<ShopCardViewModel> _allCards = new();
        public IObservableCollection<ShopCardViewModel> AllCards => _allCards;


        
        private bool EnableDebugLogs;
        


        public GameStoreService(
            DIContainer rootContainer,
            IObservableCollection<ShopCardProxy> cardProxies,
            Dictionary<IAPId, IAPConfig> iapConfigs)
        {
            _rootContainer = rootContainer;
            _cardProxies = cardProxies;
            
            // заполняем список 
            foreach (var configs in iapConfigs)
            {
                IapConfigsMap[configs.Key.Guid] = configs.Value;
            }
        }


        public void ShowWindow()
        {
            
            // #1 создаем карточки и связыввем с прокси
            foreach (var cardProxy in _cardProxies)
            {
                CreateCardViewModel(cardProxy);
            }
            
            
            // #2 создаем сам виджет
            var gameStoreViewModel = new GameStoreViewModel(this);
            ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.GameStore, gameStoreViewModel, _ =>
            {
                _gameStoreWindow = _.GetComponent<GameStoreWindow>();
                //_gameStoreWindow.OnShow(gameStoreViewModel);
                _gameStoreWindow.onDisable = () =>
                {
                    _gameStoreWindow.gameObject.SetActive(false);
                    _shopController.Close();
                    _allCards = new();
                    _createdCardsMap = new();
                };
            });

            _shopController.Open();
        }


        void CreateCardViewModel(ShopCardProxy cardProxy)
        {
            var iapCardViewModel = new ShopCardViewModel(
                cardProxy, 
                IapConfigsMap[cardProxy.ConfigId], 
                this,
                _shopController.OnBuyClicked);
            _allCards.Add(iapCardViewModel);
            _createdCardsMap[cardProxy.Id] = iapCardViewModel;
        }

        void RemovCardViewModel(ShopCardProxy cardProxy)
        {
            if (_createdCardsMap.TryGetValue(cardProxy.Id, out var iapCardViewModel))
            {
                _allCards.Remove(iapCardViewModel);
                _createdCardsMap.Remove(cardProxy.Id);
            }
        }


        
        public void SDKInitialize(Action onComplete)
        {
            _purchaseService = new PurchaseService(_rootContainer, onComplete, IapConfigsMap);
            _shopController = Object.FindAnyObjectByType<ShopController>();
            _shopController.Initialize(_rootContainer, this, _purchaseService);
        }

        public void SDKInitialized() {}
    }
}
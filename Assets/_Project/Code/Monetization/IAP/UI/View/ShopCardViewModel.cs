using System;
using Galactic1.Code.GameDatabase.Registries;
using R3;

namespace Galactic1.UI.Shop
{
    public class ShopCardViewModel
    {
        private readonly ShopCardProxy _cardProxy;
        public readonly IAPConfig _iapConfig;
        public readonly GameStoreService _gameStoreService;
        
        
        public readonly IAPId ConfigId;
        public readonly string ProductId;
        public readonly int Id;
        public readonly Action<IAPConfig, ShopCardUIBase> OnBuy;
        
        public ReadOnlyReactiveProperty<int> Limit { get; }
        
        
        public ShopCardViewModel(
            ShopCardProxy cardProxy, 
            IAPConfig iapConfig,
            GameStoreService gameStoreService,
            Action<IAPConfig, ShopCardUIBase> onBuy)
        {
            _cardProxy = cardProxy;
            _iapConfig = iapConfig;
            _gameStoreService = gameStoreService;
            OnBuy = onBuy;
            
            ConfigId = iapConfig.Id;
            ProductId = iapConfig.ProductId;
            Id = cardProxy.Id;

            Limit = cardProxy.Limit;
        }
    }
}
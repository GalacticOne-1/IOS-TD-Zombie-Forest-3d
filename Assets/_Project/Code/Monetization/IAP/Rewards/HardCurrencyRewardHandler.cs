
using System.Collections.Generic;
using Galactic1.Code.Core.State;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Systems.Economy;
using Galactic1.Configs;
using Galactic1.Configs.Galactic1.Code.GameDatabase;
using Galactic1.Core;
using Galactic1.UI.Core;

namespace Galactic1.UI.Shop.Rewards
{
    public class HardCurrencyRewardHandler : RewardHandlerBase, IShopRewardHandler
    {
        public HardCurrencyRewardHandler(DIContainer container) : base(container) {}

        public ShopRewardType RewardType => ShopRewardType.HardCurrency;

        public void Grant(IAPConfig config, ShopCardUIBase view)
        {
            DLog.Alert($">>>>> HardCurrencyRewardHandler.Grant {config} <<<<<");
            
            if(view != null)
            {
                // PurchaseController.I.PurchaseRegular(
                //     config.rewardValue, 
                //     ShopController.I.currentItem.transform.position,
                //     EStat.Hard);
                // _rootContainer.Resolve<BankResourceService>().AddResource(
                //     EBankResourceType.CurrencyPremium,
                //     config.RewardSingle.rewardValue);
                _rootContainer.Resolve<IEconomyService>().Add(
                    EBankResourceType.CurrencyPremium,
                    config.RewardSingle.rewardValue);
            }
            // если карточка потерялась, просто зачисляем награду
            else
            {
                // _rootContainer.Resolve<BankResourceService>().AddResource(
                //     EBankResourceType.CurrencyPremium,
                //     config.RewardSingle.rewardValue);
                _rootContainer.Resolve<IEconomyService>().Add(
                    EBankResourceType.CurrencyPremium,
                    config.RewardSingle.rewardValue);
            }

            
            // *** отключаем рекламу
            if(config.DisableAd)
            {
                StateWriter.Write(_rootContainer.Resolve<IGameStateProvider>().GameStateProxy.ADState,
                    (ref CGameStateAD ad) =>
                    {
                        ad.ShowAutoAds = false;
                    });
            }
        }
        
        public List<ShopRewardItemData> BuildRewardItems(IAPConfig config)
        {
            return new List<ShopRewardItemData>()
            {
                new (GameContent.Currency.Get(GameIdProvider.Coins), null, config.RewardSingle.rewardValue)
            };
        }
    }
}
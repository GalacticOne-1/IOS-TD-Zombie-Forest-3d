
using System;
using Galactic1.Code.Core.Ads;
using Galactic1.Code.Core.State;
using Galactic1.Code.Systems.Daily;
using Galactic1.Configs;
using Galactic1.Core;

namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Управляет суточными лимитами рекламы.
    /// Связан с GameState.
    /// </summary>
    public class AdEconomyService : IDailyResetRule
    {
        private IGameStateProvider gameStateProvider;

        public int DailyLimitConfig { get; private set; } // максимальный лимит для дня
        public int RemainingLimit => gameStateProvider.GameStateProxy.ADState.Value.RemainingLimit;
        
        public event Action OnEconomyChanged;
        
        
        public AdEconomyService(DIContainer container)
        {
            var configProvider = container.Resolve<IConfigProvider>();
            gameStateProvider = container.Resolve<IGameStateProvider>();

            DailyLimitConfig = configProvider.Get<GameConfig>().Ad.dailyLimit;
        }




        public bool HasDailyQuota() => RemainingLimit > 0;

        public void RegisterShow()
        {
            StateWriter.Write(gameStateProvider.GameStateProxy.ADState, (ref CGameStateAD ad) =>
            {
                ad.RemainingLimit = Math.Max(0, ad.RemainingLimit - 1);
            });
            OnEconomyChanged?.Invoke();
        }
        
        /// <summary>
        /// Вызывается DailyResetService при наступлении нового дня.
        /// </summary>
        public void ExecuteReset()
        {
            // reset ad limit
            StateWriter.Write(gameStateProvider.GameStateProxy.ADState, (ref CGameStateAD ad) =>
            {
                ad.RemainingLimit = DailyLimitConfig;
            });
            OnEconomyChanged?.Invoke();
        }

    }
}
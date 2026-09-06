using System;
using Galactic1.Code.Notification;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Core.Enums;
using Galactic1.Core.Results;

namespace Galactic1.Game.Runtime.Trader
{
    /// <summary>
    /// Scene-адаптер трейдера.
    ///
    /// Прокси между UI и TraderFacilityRuntime.
    /// Не содержит логики генерации/выдачи предметов.
    /// Не создаёт DTO.
    /// Только команды + события.
    /// </summary>
    public sealed class CampTraderSceneAdapter : IFacilitySceneAdapter
    {
        private readonly ICampTraderRuntime _runtime;

        public FacilityType Type => _runtime.Type;

        public event Action OnStateChanged
        {
            add => _runtime.OnStateChanged += value;
            remove => _runtime.OnStateChanged -= value;
        }

        public CampTraderSceneAdapter(ICampTraderRuntime runtime)
        {
            _runtime = runtime;
        }

        // =========================================================
        // COMMANDS
        // =========================================================

        public bool CanBuy(string offerId, PurchaseType type)
            => _runtime.CanBuy(offerId, type);

        /// <summary>
        /// Попытка купить предмет.
        /// Runtime сам проверяет:
        /// - существует ли оффер
        /// - хватает ли валюты
        /// </summary>
        public (NotificationResult result, Action finishAction) TryBuy(string offerId, PurchaseType type)
        {
            if (string.IsNullOrEmpty(offerId))
                return (NotificationResult.Fail(NotificationFailReason.None), null);

            var response = _runtime.TryBuy(offerId, type);

            // * пушим игрока о фейле
            if (!response.Item1.Success)
                ServiceLocator.Current.Get<INotificationService>().Push(response.Item1.FailReason);

            return response;
        }
    }
}

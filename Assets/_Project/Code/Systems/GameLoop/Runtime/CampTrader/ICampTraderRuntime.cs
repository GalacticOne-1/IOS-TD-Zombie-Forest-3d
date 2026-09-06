using System;
using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Core.Results;
using Galactic1.Meta.Configs.Trader;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Интерфейс runtime трейдера.
    /// Аналог IRecruitmentTavernRuntime, но без генерации — офферы статичны (из конфига).
    /// </summary>
    public interface ICampTraderRuntime
    {
        FacilityType Type { get; }
        IReadOnlyList<TraderOfferConfig> Offers { get; }
        event Action OnStateChanged;
        int Level { get; }

        bool CanBuy(string offerId, PurchaseType type);

        /// <summary>
        /// Попытка купить предмет.
        /// Runtime сам проверяет валюту, списывает её и возвращает finishAction,
        /// который непосредственно выдаёт предмет (вызывается из UI, как и в таверне).
        /// </summary>
        (NotificationResult, Action) TryBuy(string offerId, PurchaseType type);
    }
}

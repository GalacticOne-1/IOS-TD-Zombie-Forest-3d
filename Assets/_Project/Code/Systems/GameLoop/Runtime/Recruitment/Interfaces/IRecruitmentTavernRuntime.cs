using System;
using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Core.Results;
using Galactic1.Game.Buildings.Proxy;

namespace Galactic1.Code.Systems.Runtime.Building
{
    public interface IRecruitmentTavernRuntime
    {
        FacilityType Type { get; }
        IReadOnlyList<RecruitOfferProxy> Offers { get; }
        event Action OnStateChanged;
        int Level { get; }

        int RefreshPremiumCost { get; }
        int DaysUntilRefresh { get; }

        
        bool CanRefreshPremium();
        bool TryPremiumRefresh();
        bool HasFreeSlot();
        bool CanRecruit(string offerId);
        (NotificationResult, Action) TryRecruit(string offerId, PurchaseType type);
        
    }
}
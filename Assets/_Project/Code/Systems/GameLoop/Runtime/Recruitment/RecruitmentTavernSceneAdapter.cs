using System;
using Galactic1.Code.Notification;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Core.Enums;
using Galactic1.Core.Results;

namespace Galactic1.Game.Runtime.Recruitment
{
    /// <summary>
    /// Scene-адаптер таверны.
    /// 
    /// Прокси между UI и RecruitmentTavernRuntime.
    /// Не содержит логики генерации.
    /// Не создаёт DTO.
    /// Только команды + события.
    /// </summary>
    public sealed class RecruitmentTavernSceneAdapter : IFacilitySceneAdapter
    {
        private readonly IRecruitmentTavernRuntime _runtime;

        public FacilityType Type => _runtime.Type;

        public event Action OnStateChanged
        {
            add => _runtime.OnStateChanged += value;
            remove => _runtime.OnStateChanged -= value;
        }

        public RecruitmentTavernSceneAdapter(
            IRecruitmentTavernRuntime runtime)
        {
            _runtime = runtime;
        }

        // =========================================================
        // COMMANDS
        // =========================================================


        public int DaysUntilRefresh() => _runtime.DaysUntilRefresh;
        
        public int GetPremiumRefreshCost() => _runtime.RefreshPremiumCost;

        public bool CanPremiumRefresh() => _runtime.CanRefreshPremium();
        
        public bool TryPremiumRefresh() => _runtime.TryPremiumRefresh();


        /// <summary>
        /// Проверяет свободное место
        /// </summary>
        /// <returns></returns>
        public bool NoFreeSlot()
        {
            var freeSlot = _runtime.HasFreeSlot();
            if (freeSlot)
                return false;

            ServiceLocator.Current.Get<INotificationService>().Push(NotificationFailReason.NoFreeCampSlots);
            return true;
        }

        /// <summary>
        /// Попытка нанять юнита.
        /// Runtime сам проверяет:
        /// - есть ли место в лагере
        /// - хватает ли валюты
        /// - существует ли оффер
        /// </summary>
        public (NotificationResult result, Action finishAction) TryRecruit(string offerId, PurchaseType type)
        {
            if (string.IsNullOrEmpty(offerId))
                return (NotificationResult.Fail(NotificationFailReason.None), null);
            
            var response = _runtime.TryRecruit(offerId, type);

            // * пушим игрока о фейле
            if (!response.Item1.Success)
            {
                ServiceLocator.Current.Get<INotificationService>().Push(response.Item1.FailReason);
            }

            return response;
        }
    }
}
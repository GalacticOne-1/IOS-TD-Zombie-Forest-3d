using System;
using Galactic1.Code.Notification;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Code.Systems.Squad;
using Galactic1.Code.Systems.World.Threats;
using Galactic1.Core.Results;
using UnityEngine;

namespace Galactic1.Code.Systems.CampDefense.Preparation
{
    /// <summary>
    /// Координатор готовности "защиты лагеря" перед атакой орды.
    ///
    /// НЕ управляет угрозами — WorldThreatService остаётся единственным
    /// источником истины о Threat. НЕ знает деталей UI.
    /// Отвечает только на вопрос "доступна ли сейчас защита лагеря?"
    /// и уведомляет подписчиков об изменении этого состояния.
    /// </summary>
    public sealed class CampDefensePreparationService : IGameService
    {
        private WorldThreatService _worldThreatService;
        private GameTimeService _gameTimeService;
        private SquadValidationService _squadValidation;
        private CampDefenseImmediateDefeatService _immediateDefeat;

        public bool IsDefenseAvailable { get; private set; }

        public event Action<bool> DefenseAvailabilityChanged;

        
        
        public void Activate(DIContainer container)
        {
            _worldThreatService = container.Resolve<WorldThreatService>();
            _gameTimeService = container.Resolve<GameTimeService>();
            _squadValidation = container.Resolve<SquadValidationService>();
            _immediateDefeat = container.Resolve<CampDefenseImmediateDefeatService>();

            _worldThreatService.ThreatStageChanged += OnThreatStateChanged;
            _worldThreatService.ThreatActivated += OnThreatStateChanged;

            // восстанавливаем состояние сразу — покрывает и новую игру,
            // и загрузку сохранения с уже существующей угрозой
            Refresh();
        }

        /// <summary>
        /// Ничего не вычисляет — только просит существующий pipeline перехода
        /// на локацию открыть лагерь в режиме обороны
        /// (см. CoreRegistrations: EventBus&lt;CampDefenseRequestEvent&gt;).
        /// </summary>
        public void StartDefense()
        {
            switch (_squadValidation.ValidateForCampDefense())
            {
                case SquadValidationResult.Success:
                    EventBus<CampDefenseRequestEvent>.Raise(new CampDefenseRequestEvent());
                    break;

                case SquadValidationResult.EmptySquad:
                    ServiceLocator.Current.Get<INotificationService>().Push(NotificationFailReason.SquadIsEmpty);
                    break;

                case SquadValidationResult.NoUnitsInCamp:
                    _immediateDefeat.TriggerImmediateDefeat();
                    break;
            }
        }

        /// <summary>
        /// Вызывается после завершения Camp Defense (см. CampDefenseScenario.Cleanup()).
        /// Замыкает цикл угрозы: пропускает день, разрешает текущую угрозу
        /// (WorldThreatService.ResolveThreat() сам создаёт следующую) и снимает
        /// доступность защиты.
        /// </summary>
        public void CompleteDefense()
        {
            _gameTimeService.SkipToNextDay(TimeAdvanceReason.CampDefense);
            _worldThreatService.ResolveThreat();

            SetDefenseAvailable(false);
        }

        private void OnThreatStateChanged(Threat threat) => Refresh();

        private void Refresh()
        {
            int remainingDays = _worldThreatService.GetCurrentThreatRemainingDays();

            SetDefenseAvailable(remainingDays == 0);
        }

        private void SetDefenseAvailable(bool isAvailable)
        {
            if (IsDefenseAvailable == isAvailable)
                return;

            IsDefenseAvailable = isAvailable;
            DefenseAvailabilityChanged?.Invoke(IsDefenseAvailable);
        }
    }
}
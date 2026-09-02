using System;
using Galactic1.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.GameTime
{
    /// <summary>
    /// GameTimeService — единственный источник правды о времени в игре.
    ///
    /// Отвечает за:
    /// - текущий игровой день
    /// - продвижение времени (в днях)
    /// - генерацию событий DayPassed
    /// - начало и завершение временных интервалов (рейд, ожидание, скрипты)
    ///
    /// ВАЖНО:
    /// - Время в игре дискретное (по дням)
    /// - Ни одна система не имеет права изменять день напрямую
    /// - Все изменения времени проходят ТОЛЬКО через этот сервис
    /// </summary>
    public sealed class GameTimeService_old : IGameService
    {
        private DIContainer _container;
        
        
        /// <summary>
        /// Текущий игровой день (начинается с 1)
        /// </summary>
        public int CurrentDay { get; private set; }
        /// <summary>
        /// Оставшиеся часы в текущем дне (0–24)
        /// </summary>
        public int RemainingHour { get; private set; }

        /// <summary>
        /// Идёт ли сейчас процесс продвижения времени
        /// (нужно для защиты от вложенных вызовов)
        /// </summary>
        public bool IsTimeAdvancing { get; private set; }

        /// <summary>
        /// Событие — день прошёл
        /// Вызывается для КАЖДОГО дня, даже при пропуске нескольких дней сразу
        /// </summary>
        public event Action<DayPassedEvent> DayPassed;

        /// <summary>
        /// Событие — начался процесс продвижения времени
        /// (например: рейд на 3 дня или ручной пропуск)
        /// </summary>
        public event Action<TimeAdvanceStartedEvent> TimeAdvanceStarted;

        /// <summary>
        /// Событие — завершён процесс продвижения времени
        /// Используется для формирования Post-Raid / Time Skip отчёта
        /// </summary>
        public event Action<TimeAdvanceFinishedEvent> TimeAdvanceFinished;


        
        
        /// <summary>
        /// Инициализация сервиса
        /// </summary>
        public void Activate(DIContainer container)
        {
            _container = container;
            
            // устанавливаем текущее состояние времени в мире
            var stateProxy = _container.Resolve<IGameStateProvider>().GameStateProxy;
            
            CurrentDay = stateProxy.GameLoopContext.CurrentDay.Value;
            RemainingHour = stateProxy.GameLoopContext.RemainingHour.Value;
            
            if (RemainingHour <= 0 || RemainingHour > 24)
                RemainingHour = 24;
            
            IsTimeAdvancing = false;
        }



        /// <summary>
        /// Продвинуть время на указанное количество дней
        /// Используется для:
        /// - ручного пропуска дней
        /// - ускорения производства
        /// - ожидания лечения
        /// </summary>
        public void AdvanceDays(int days, TimeAdvanceReason reason)
        {
            if (days <= 0 || IsTimeAdvancing)
                return;

            IsTimeAdvancing = true;

            int startDay = CurrentDay;

            TimeAdvanceStarted?.Invoke(new TimeAdvanceStartedEvent(
                startDay: startDay,
                daysPlanned: days,
                reason: reason
            ));

            for (int i = 0; i < days; i++)
            {
                AdvanceSingleDay(reason);
            }

            int endDay = CurrentDay;

            IsTimeAdvancing = false;

            TimeAdvanceFinished?.Invoke(new TimeAdvanceFinishedEvent(
                startDay: startDay,
                endDay: endDay,
                daysPassed: endDay - startDay,
                reason: reason
            ));
            
            SaveTimeState();
        }
        
        /// <summary>
        /// Продвижение времени на часы.
        /// Используется после рейдов, перемещений, ожиданий.
        /// </summary>
        public void SpendHours(int hours, TimeAdvanceReason reason)
        {
            if (hours <= 0 || IsTimeAdvancing)
                return;

            IsTimeAdvancing = true;

            int startDay = CurrentDay;
            int totalDaysPassed = 0;

            TimeAdvanceStarted?.Invoke(new TimeAdvanceStartedEvent(
                startDay: CurrentDay,
                daysPlanned: Mathf.CeilToInt(hours / 24f),
                reason: reason
            ));

            int hoursToSpend = hours;

            while (hoursToSpend > 0)
            {
                if (RemainingHour > hoursToSpend)
                {
                    RemainingHour -= hoursToSpend;
                    hoursToSpend = 0;
                }
                else
                {
                    hoursToSpend -= RemainingHour;
                    AdvanceSingleDay(reason);
                    totalDaysPassed++;
                }
            }

            SaveTimeState();

            IsTimeAdvancing = false;

            TimeAdvanceFinished?.Invoke(new TimeAdvanceFinishedEvent(
                startDay: startDay,
                endDay: CurrentDay,
                daysPassed: totalDaysPassed,
                reason: reason
            ));
        }


        
        /// <summary>
        /// Продвижение времени через рейд (дни + часы)
        /// </summary>
        public void AdvanceByRaid(RaidTimeData raid)
        {
            if (raid == null || raid.DurationDays <= 0)
                return;

            int hours = Mathf.CeilToInt(raid.DurationDays * 24f);
            SpendHours(hours, TimeAdvanceReason.Raid);
        }

        
        
        #region Internal

        /// <summary>
        /// Продвижение ровно одного дня
        /// </summary>
        private void AdvanceSingleDay(TimeAdvanceReason reason)
        {
            CurrentDay++;
            RemainingHour = 24;

            DayPassed?.Invoke(new DayPassedEvent(
                CurrentDay,
                reason
            ));
        }

        private void SaveTimeState()
        {
            var stateProxy = _container.Resolve<IGameStateProvider>().GameStateProxy;
            stateProxy.GameLoopContext.CurrentDay.Value = CurrentDay;
            stateProxy.GameLoopContext.RemainingHour.Value = RemainingHour;
            ServiceLocator.Current.Get<IGameStateProvider>().SaveGameState();
        }

        #endregion
    }
}

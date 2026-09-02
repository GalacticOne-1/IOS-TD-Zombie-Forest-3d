using System;
using Galactic1.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.GameTime
{
    /// <summary>
    /// GameTimeService — единственный источник правды о времени.
    /// Модель времени основана на TotalWorldHours (линейное время).
    /// День и оставшиеся часы вычисляются производно.
    /// </summary>
    public sealed class GameTimeService : IGameService
    {
        private DIContainer _container;

        /// <summary>
        /// Абсолютное время мира в часах с начала игры.
        /// Это единственный источник истины.
        /// </summary>
        public int TotalWorldHours { get; private set; }

        /// <summary>
        /// Текущий игровой день (начинается с 1).
        /// </summary>
        public int CurrentDay => (TotalWorldHours / 24) + 1;

        /// <summary>
        /// Оставшиеся часы до конца текущего дня.
        /// </summary>
        public int RemainingHour
        {
            get
            {
                int mod = TotalWorldHours % 24;
                return mod == 0 ? 24 : 24 - mod;
            }
        }

        /// <summary>
        /// Идёт ли процесс продвижения времени.
        /// </summary>
        public bool IsTimeAdvancing { get; private set; }

        // ================= EVENTS =================

        /// <summary>
        /// Событие — прошло N часов.
        /// Основное событие для производства.
        /// </summary>
        public event Action<int, TimeAdvanceReason> HoursPassed;

        /// <summary>
        /// Событие — наступил новый день.
        /// Генерируется производно.
        /// </summary>
        public event Action<DayPassedEvent> DayPassed;

        public event Action<TimeAdvanceStartedEvent> TimeAdvanceStarted;
        public event Action<TimeAdvanceFinishedEvent> TimeAdvanceFinished;

        // ================= ACTIVATE =================

        public void Activate(DIContainer container)
        {
            _container = container;

            var stateProxy = _container
                .Resolve<IGameStateProvider>()
                .GameStateProxy;

            int savedDay = stateProxy.GameLoopContext.CurrentDay.Value;
            int savedRemaining = stateProxy.GameLoopContext.RemainingHour.Value;

            if (savedRemaining <= 0 || savedRemaining > 24)
                savedRemaining = 24;

            // Восстанавливаем TotalWorldHours
            TotalWorldHours =
                ((savedDay - 1) * 24) +
                (24 - savedRemaining);

            IsTimeAdvancing = false;
        }

        // ================= PUBLIC API =================

        /// <summary>
        /// Потратить указанное количество часов.
        /// </summary>
        public void SpendHours(int hours, TimeAdvanceReason reason)
        {
            if (hours <= 0 || IsTimeAdvancing)
                return;

            IsTimeAdvancing = true;

            int startDay = CurrentDay;
            int startHours = TotalWorldHours;

            TimeAdvanceStarted?.Invoke(
                new TimeAdvanceStartedEvent(
                    startDay,
                    Mathf.CeilToInt(hours / 24f),
                    reason));

            AdvanceInternal(hours, reason);

            int endDay = CurrentDay;
            int daysPassed = endDay - startDay;

            SaveTimeState();

            IsTimeAdvancing = false;

            TimeAdvanceFinished?.Invoke(
                new TimeAdvanceFinishedEvent(
                    startDay,
                    endDay,
                    daysPassed,
                    reason));
        }

        /// <summary>
        /// Продвинуть N полных дней.
        /// </summary>
        public void AdvanceDays(int days, TimeAdvanceReason reason)
        {
            if (days <= 0)
                return;

            SpendHours(days * 24, reason);
        }

        /// <summary>
        /// Перемотать до начала следующего календарного дня.
        /// </summary>
        public void SkipToNextDay(TimeAdvanceReason reason)
        {
            int hoursToNextDay = RemainingHour;

            if (hoursToNextDay <= 0)
                hoursToNextDay = 24;

            SpendHours(hoursToNextDay, reason);
        }

        /// <summary>
        /// Продвижение времени через рейд.
        /// </summary>
        public void AdvanceByRaid(RaidTimeData raid)
        {
            if (raid == null || raid.DurationDays <= 0)
                return;

            int hours = Mathf.CeilToInt(raid.DurationDays * 24f);
            SpendHours(hours, TimeAdvanceReason.Raid);
        }

        // ================= INTERNAL =================

        private void AdvanceInternal(int hours, TimeAdvanceReason reason)
        {
            int previousDay = CurrentDay;

            TotalWorldHours += hours;

            // Событие часов — главное для производства
            HoursPassed?.Invoke(hours, reason);

            int newDay = CurrentDay;

            // Генерация DayPassed для каждого дня
            for (int day = previousDay + 1; day <= newDay; day++)
            {
                DayPassed?.Invoke(
                    new DayPassedEvent(day, reason));
            }
        }

        private void SaveTimeState()
        {
            var stateProxy = _container
                .Resolve<IGameStateProvider>()
                .GameStateProxy;

            stateProxy.GameLoopContext.CurrentDay.Value = CurrentDay;
            stateProxy.GameLoopContext.RemainingHour.Value = RemainingHour;

            ServiceLocator.Current
                .Get<IGameStateProvider>()
                .SaveGameState();
        }
    }
}
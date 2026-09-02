using System.Collections.Generic;
using Galactic1.Code.Core.State;
using Galactic1.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.Daily
{
    /// <summary>
    /// Центральный сервис суточных доменных сбросов.
    /// Единственная система, которая определяет наступление нового серверного дня.
    /// </summary>
    public class TimeBoundaryService : IGameService
    {
        private readonly DIContainer _container;
        private readonly List<IDailyResetRule> rules = new();
        private readonly IGameStateProvider gameStateProvider;

        private int CurrentProcessedDay
        {
            get => gameStateProvider.GameStateProxy.Server.Value.CurrentServerDay;
            set
            {
                StateWriter.Write(gameStateProvider.GameStateProxy.Server, (ref ServerTime server) =>
                {
                    server.CurrentServerDay = value;
                });
            }
        }

        public TimeBoundaryService(DIContainer container)
        {
            _container = container;
            gameStateProvider = container.Resolve<IGameStateProvider>();

            container.Resolve<IServerTimeSync>().OnServerTimeSynced += CheckDailyReset;
        }

        /// <summary>
        /// Регистрация доменного правила суточного сброса.
        /// Вызывается при инициализации сервисов.
        /// </summary>
        public void RegisterRule(IDailyResetRule rule)
        {
            rules.Add(rule);
        }

        /// <summary>
        /// Проверяется при старте игры и далее периодически (например, раз в минуту).
        /// </summary>
        public void CheckDailyReset()
        {
            int today = _container.Resolve<IServerTimeSync>().GetServerDay();
            if (today == CurrentProcessedDay)
                return;

            Debug.Log($"[DailyReset] New day detected: {today}");

            CurrentProcessedDay = today;

            foreach (var rule in rules)
                rule.ExecuteReset();
        }
    }
}
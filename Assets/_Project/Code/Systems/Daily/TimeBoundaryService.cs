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
        /// Вызывается после каждой синхронизации времени и явно при старте игры.
        /// Безопасен к раннему вызову: если стейт или время ещё не готовы — просто выходит.
        /// </summary>
        public void CheckDailyReset()
        {
            // стейт ещё не загружен (синк мог прийти раньше LoadGameState)
            if (gameStateProvider.GameStateProxy == null)
                return;

            var sync = _container.Resolve<IServerTimeSync>();
            if (!sync.IsSynced)
                return;

            int today = sync.GetServerDay();
            if (today == CurrentProcessedDay)
                return;

            Debug.Log($"[DailyReset] New day detected: {today}");

            CurrentProcessedDay = today;

            foreach (var rule in rules)
                rule.ExecuteReset();
        }
    }
}

using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.WorldThreatConfig;
using Galactic1.Code.Systems.CampDefense.Preparation;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Configs;
using Galactic1.Core;

namespace Galactic1.Code.Systems.World.Threats
{
    /// <summary>
    /// Сервис управления угрозами в мире.
    /// Подписан на события времени (GameTimeService) и обновляет прогресс угроз.
    /// Игрок получает сигналы, но не точные таймеры.
    /// </summary>
    public class WorldThreatService : IGameService
    {
        private DIContainer _container;
        private IConfigProvider _configProvider;
        private List<Threat> _activeThreats = new();

        /// <summary>
        /// Срабатывает при изменении стадии угрозы
        /// </summary>
        public event Action<Threat> ThreatStageChanged;

        /// <summary>
        /// Срабатывает, когда угроза активируется
        /// </summary>
        public event Action<Threat> ThreatActivated;

        /// <summary>
        /// Инициализация сервиса
        /// </summary>
        public void Activate(DIContainer container)
        {
            _container = container;
            
            _configProvider = container.Resolve<IConfigProvider>();

            // ! Временно !
            var timeService = ServiceLocator.Current.Get<GameTimeService>();

            var stateProxy = _container.Resolve<IGameStateProvider>().GameStateProxy;

            if (stateProxy.GameLoopContext.ThreatData.Value == null) // new
            {
                int today = timeService.CurrentDay;
                
                var config = _configProvider.Get<WorldThreatConfig>();

                int quietDays = UnityEngine.Random.Range(
                    config.InitialQuietDaysMin,
                    config.InitialQuietDaysMax + 1);

                int attackPreparationDays = UnityEngine.Random.Range(
                    config.InitialPreparationDaysMin,
                    config.InitialPreparationDaysMax + 1);

                // Создаём первую угрозу — орду зомби
                var threat = new Threat(
                    Guid.NewGuid().ToString(),
                    ThreatType.Horde,
                    today,
                    today + quietDays,
                    today + quietDays + attackPreparationDays);

                // Добавляем угрозу в сервис
                AddThreat(threat);
            }
            else // load
            {
                var td = stateProxy.GameLoopContext.ThreatData.Value;

                var threat = new Threat(
                    td.Id,
                    (ThreatType)td.Type,
                    td.CreatedAtDay,
                    td.RevealDay,
                    td.AttackDay,
                    (ThreatStage)td.Stage
                );

                AddThreat(threat);
            }

            // ! Временно !
            timeService.DayPassed += OnDayPassed;

            // Подписка на событие готовности UI
            EventBus<SceneUIReadyEvent>.Register(new EventBinding<SceneUIReadyEvent>(() =>
            {
                // пересылаем текущие угрозы всем подписчикам UI
                foreach (var threat in _activeThreats)
                {
                    DLog.Alert("=============");
                    DLog.Alert($"{threat.Type} / {threat.Stage}");

                    ThreatStageChanged?.Invoke(threat);

                    if (threat.Stage == ThreatStage.Imminent || threat.Stage == ThreatStage.Active)
                        ThreatActivated?.Invoke(threat);
                }
            }));
        }

        /// <summary>
        /// Вызывается при наступлении нового дня (подписка на GameTimeService.DayPassed)
        /// </summary>
        public void OnDayPassed(DayPassedEvent e)
        {
            var config = _configProvider.Get<WorldThreatConfig>();
                
            foreach (var threat in _activeThreats)
            {
                if (threat.Stage == ThreatStage.Resolved)
                    continue;

                // АТАКА
                if (e.Day >= threat.AttackDay && threat.Stage != ThreatStage.Active)
                {
                    threat.SetStage(ThreatStage.Active);
                    ThreatActivated?.Invoke(threat);
                    SyncProxy(threat);

                    // *** TEST
                    // по достижении орды начинает новый цикл
                    if (config.TestThreat)
                    {
                        _container.Resolve<CampDefensePreparationService>().CompleteDefense();
                    }
                    continue;
                }

                // ПРЕДУПРЕЖДЕНИЕ
                if (e.Day >= threat.RevealDay && threat.Stage == ThreatStage.Dormant)
                {
                    threat.SetStage(ThreatStage.Imminent);
                    ThreatStageChanged?.Invoke(threat);
                    SyncProxy(threat);
                }
                
                if (threat.Stage == ThreatStage.Active &&
                    e.Day > threat.AttackDay)
                {
                    EventBus<HordeAttackMissedEvent>.Raise(new HordeAttackMissedEvent());

                    ResolveThreat();
                }
            }
        }

        /// <summary>
        /// Добавляет новую угрозу в мир
        /// </summary>
        public void AddThreat(Threat threat)
        {
            _activeThreats.Add(threat);

            // === new data
            SyncProxy(threat);
        }

        public void ResolveThreat()
        {
            var config = _configProvider.Get<WorldThreatConfig>();
            
            _activeThreats = new();

            // Создаём новую угрозу с периодом тишины
            var time = ServiceLocator.Current.Get<GameTimeService>();
            int today = time.CurrentDay;

            // Период тишины
            int quietDays = UnityEngine.Random.Range(
                config.QuietDaysMin,
                config.QuietDaysMax + 1);
            
            int attackDays;

            // Балансное правило
            if (quietDays >= config.LongQuietThreshold)
            {
                // Долгая тишина → быстрая атака
                attackDays = UnityEngine.Random.Range(
                    config.FastAttackDaysMin,
                    config.FastAttackDaysMax + 1);
            }
            else
            {
                // Короткая тишина → долгая подготовка
                attackDays = UnityEngine.Random.Range(
                    config.SlowAttackDaysMin,
                    config.SlowAttackDaysMax + 1);
            }

            var nextThreat = new Threat(
                Guid.NewGuid().ToString(),
                ThreatType.Horde,
                today,
                today + quietDays,
                today + quietDays + attackDays
            );

            DLog.Alert("===== NEW THREAT =====", EDlogColor.ORANGE);
            DLog.Alert($"created day: {nextThreat.CreatedAtDay}; reveal day: {nextThreat.RevealDay}",
                EDlogColor.ORANGE);

            AddThreat(nextThreat);
        }

        /// <summary>
        /// Возвращает все активные угрозы
        /// </summary>
        public IEnumerable<Threat> GetActiveThreats() => _activeThreats;

        /// <summary>
        /// Возвращает текущую угрозу
        /// </summary>
        public Threat GetCurrentThreat()
        {
            Threat highestPriorityThreat = null;

            foreach (var threat in _activeThreats)
            {
                if (threat.Stage == ThreatStage.Dormant || threat.Stage == ThreatStage.Resolved)
                    continue;

                if (highestPriorityThreat == null || threat.Stage > highestPriorityThreat.Stage)
                    highestPriorityThreat = threat;
            }

            return highestPriorityThreat;
        }

        /// <summary>
        /// Сколько полных дней осталось до атаки текущей угрозы.
        ///
        /// Важно: считаем через целые игровые дни (AttackDay - CurrentDay),
        /// а НЕ через Threat.GetRemainingDays(TotalWorldHours) / 24 — та формула
        /// оперирует часами и даёт неверный результат, если TotalWorldHours
        /// не выровнен на границу суток (а он почти никогда не выровнен: часы
        /// тратятся частями через SpendHours на производстве/крафте в течение дня,
        /// и то же самое происходит при восстановлении времени после загрузки
        /// сохранения, сделанного в середине дня). Из-за целочисленного деления
        /// на 24 значение "1 день до атаки" в таком случае проскакивалось,
        /// и CampDefensePreparationService никогда не включал defenseCampButton.
        ///
        /// День-ориентированное сравнение от этого не зависит.
        /// Возвращает int.MaxValue, если активной угрозы нет.
        /// </summary>
        public int GetCurrentThreatRemainingDays()
        {
            var threat = GetCurrentThreat();
            if (threat == null)
                return int.MaxValue;

            var gameTime = ServiceLocator.Current.Get<GameTimeService>();

            return threat.AttackDay - gameTime.CurrentDay;
        }

        void SyncProxy(Threat threat)
        {
            var stateProxy = _container.Resolve<IGameStateProvider>().GameStateProxy;
            var threatSaveData = stateProxy.GameLoopContext.ThreatData.Value;

            threatSaveData = new ThreatSaveData()
            {
                Id = threat.Id,
                Type = (int)threat.Type,
                Stage = (int)threat.Stage,
                CreatedAtDay = threat.CreatedAtDay,
                RevealDay = threat.RevealDay,
                AttackDay = threat.AttackDay,
            };

            stateProxy.GameLoopContext.ThreatData.Value = threatSaveData;
        }
    }
}
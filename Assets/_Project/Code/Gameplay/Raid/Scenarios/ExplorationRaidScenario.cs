using System.Collections.Generic;
using Galactic1.Code.Core;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Raid.Mission;
using Galactic1.Core.GameSession;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Meta.Enemy;
using Galactic1.RaidLoot.Navigation;
using Galactic1.RaidLoot.Services;
using UnityEngine;

namespace Galactic1.Code.Systems.Raid.Scenarios
{
    /// <summary>
    /// Текущее поведение рейда — полностью реализовано общим пайплайном
    /// RaidInProgressState (loot, ambient population, exit zones включены
    /// через Options). Сценарий не создаёт и не хранит эти системы.
    /// </summary>
    public class ExplorationRaidScenario : IRaidScenario
    {
        private readonly DIContainer _container;
        private readonly GameLoopContext _gameLoopContext;
        private LootObstacleBuilder _obstacleBuilder;
        
        
        public EnemyAIProfile AIProfile => EnemyAIProfile.Raid;
        
        public ScenarioOptions Options { get; } = new ScenarioOptions
        {
            UseDefenseFacilities = false,
            UseWaveSpawner = false,
            UseAmbientPopulation = true,
            UseLoot = true,
            UseExitZones = true,
            UseTransport = true,
        };

        public ExplorationRaidScenario(DIContainer container)
        {
            _container = container;
            
            _gameLoopContext = _container.Resolve<GameSession>().GameLoopContext;
        }


        
        
        public void Configure(RaidRuntime raid)
        {
            // Squad/Transport уже собираются в RaidInProgressState.Enter() как и раньше —
            // exploration ничего доп. не требует.

            _gameLoopContext.CurrentRaid.DefenseFacilities = null;
        }

        public void OnSceneLoaded(SceneSessionDefinition scene)
        {
            // К этому моменту LootContainerSceneLifecycleSystem.SetSceneReady()
            // уже отработал в RaidInProgressState.BuildLootSystem — контейнеры заспавнены.
            var raid = _container.Resolve<GameSession>().GameLoopContext.CurrentRaid;

            if (raid.CurrentRaidLootContainer != null)
            {
                _obstacleBuilder = new LootObstacleBuilder(raid.CurrentRaidLootContainer);
                _obstacleBuilder.Build();
            }
        }

        public void OnBattleStarted()
        {
            // Место для exploration objectives, если появятся.
        }

        public void OnBattleFinished()
        {
        }

        public void Cleanup()
        {
            // Сценарий сам ничего не создавал — нечего освобождать.
            // Loot/Ambient/ExitZones освобождает RaidInProgressState.Exit().
        }

        public void ApplyResults()
        {

        }


        public void ExitFromLocation()
        {
            // после рейда всегда выходим на карту
            EventBus<WorldMapSceneRequestEvent>.Raise(new WorldMapSceneRequestEvent());
        }
        
        
        public bool ArePlayerForcesDestroyed(MissionStateProvider state)
        {
            return state.IsSquadDestroyed();
        }

        public MissionResult EvaluateMission(MissionContext context)
        {
            var result = new MissionResult();
            
            // todo
            // пока при применяется любой результат даже смерть 

            if (context.PlayerForcesDestroyed)  // поражение при гибель отряда
            {
                result = MissionResult.Defeat;
                _container.Resolve<GameSession>().GameLoopContext.CurrentRaid.Status = RaidStatus.Failed;
                result.EndReason = RaidEndReason.ObjectivesCompleted;  // ???
                
#if UNITY_EDITOR
                Debug.LogError($"Mission Defeat: squad destroyed");
#endif
            }
            
            else if (context.ExitReached)       // победа через зону выхода
            {
                result = MissionResult.Victory;
                _container.Resolve<GameSession>().GameLoopContext.CurrentRaid.Status = RaidStatus.Completed;
                result.EndReason = RaidEndReason.ObjectivesCompleted;
                
#if UNITY_EDITOR
                //Debug.LogError($"Mission Victory: squad exited");
#endif
            }
            else
                return MissionResult.Running;
           
            
            return result;
        }



        public RaidResultProxy BuildRaidResult(RaidRuntime raid, MissionResult mission)
        {
            bool isSuccess = mission.Status == MissionStatus.Victory;
            int killedEnemies = 0;

            List<RaidRewardLootData> lootReceived = new();
            if (raid.Status == RaidStatus.Completed)
            {
                // ── Loot: читаем из буфера ────────────────────────────────────
                var mapper = new LootResultMapper();
                lootReceived = raid.LootBuffer != null
                    ? mapper.Map(raid.LootBuffer)
                    : new List<RaidRewardLootData>();
                // ─────────────────────────────────────────────────────────────
            }

            int experienceGained = raid.CalculateExperience(isSuccess, killedEnemies);

            return new RaidResultProxy(new RaidResultData
            {
                IsSuccess = isSuccess,
                KilledEnemies = killedEnemies,
                ExperienceGained = experienceGained,
                LootReceived = lootReceived,
                ResourcesLost = new()
            });
        }
        
        
    }
}
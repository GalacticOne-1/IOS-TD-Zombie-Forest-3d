using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Enemies.Spawning;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Systems.Runtime.Enemy;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Code.Systems.Raid.Enemies
{
    /// <summary>
    /// Runtime-данные одного зомби в рейде.
    /// Аналог RaidUnitRuntime для игрока.
    ///
    /// Живёт с момента спавна до деспавна (смерть, конец рейда).
    /// Создаётся ZombieRuntimeFactory.
    /// Регистрируется в RaidRuntime.Enemies.
    /// </summary>
    public sealed class EnemyRuntime : IEnemyUnitRuntime
    {
        // ── IUnitRuntimeBase ───────────────────────────────────────────
        public EnemyRuntimeDefinition Definition { get; } // Runtime должен иметь immutable definition.
        public UnitGameplayDefinition RuntimeDefinition => Definition;
        public EnemyAIProfile AIProfile => Definition.AIProfile;
        public string Id { get; }
        public string DisplayName { get; }
        public int TeamId { get; } = 1; // 1 = враги

        // ============================================================
        // RUNTIME
        // ============================================================
        public IUnitStatsRuntime Stats { get; }
        public ActiveEffectsRuntime Effects { get; }
        public CooldownTracker Cooldowns { get; }
        public bool IsInCombat { get; private set; }

        // ── IEnemyUnitRuntime ──────────────────────────────────────────

        public EnemyId EnemyId => Definition.EnemyId;

        public string PrefabId => Definition.PrefabId;

        public float ThreatLevel => Definition.ThreatLevel;

        public string LootTableId => Definition.LootTableId;

        public bool IsElite => Definition.IsElite;

        public bool IsDead => Stats.IsDead;

        public event Action OnDeath;

        //private readonly EnemyStatsRuntime _stats;

        public Vector3 SpawnPosition { get; }
        
        public float WanderRadius { get; }
        
        public SpawnSource SpawnSource { get; }

        // ─────────────────────────────────────────────────────────────────
        //  Constructor — вызывается из ZombieRuntimeFactory
        // ─────────────────────────────────────────────────────────────────

        public EnemyRuntime(
            EnemyRuntimeDefinition definition, 
            Vector3 spawnPosition,
            float wanderRadius = 0f,
            SpawnSource spawnSource = SpawnSource.Static)
        {
            Definition = definition;

            // id юнита для использования во всех системах
            Id = Guid.NewGuid().ToString();
            // ─────────────────────────────────────────────────────────────────
            // ─────────────────────────────────────────────────────────────────
            
            
            

            DisplayName = definition.DisplayName;
            SpawnPosition = spawnPosition;
            WanderRadius = wanderRadius;
            SpawnSource = spawnSource;

            Stats = new EnemyStatsRuntime(
                $"{Id}",
                new Dictionary<StatId, float>(definition.StatsSnapshot.Stats),
                new EmptyEquipmentStatsProvider());


            Effects = new ActiveEffectsRuntime();
            Cooldowns = new CooldownTracker();

            Stats.OnDeath += HandleDeath;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Tick — вызывается из ZombieSceneEntity или RaidRuntime.Tick
        // ─────────────────────────────────────────────────────────────────

        public void Tick(float dt)
        {
            Effects.Tick(dt);
            Cooldowns.Tick(dt);
        }


        // ─────────────────────────────────────────────────────────────────
        //  Dispose
        // ─────────────────────────────────────────────────────────────────

        public void Dispose()
        {
            Stats.OnDeath -= HandleDeath;
        }

        private void HandleDeath()
        {
            DLog.Alert("Enemy killed", EDlogColor.ORANGE);
            IsInCombat = false;
            OnDeath?.Invoke();
            EventBus<EnemyKilledEvent>.Raise(new EnemyKilledEvent(this));
        }
    }
}
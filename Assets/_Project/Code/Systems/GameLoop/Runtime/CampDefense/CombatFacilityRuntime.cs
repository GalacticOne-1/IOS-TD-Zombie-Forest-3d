using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.CampDefense;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Configs;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Базовый runtime для всех боевых сооружений.
    ///
    /// Используется:
    /// - Main Base
    /// - Wall
    /// - Gate
    /// - Turret
    /// - Barricade
    /// - Generator
    ///
    /// Полностью совместим с DamagePipeline благодаря
    /// реализации IUnitRuntimeBase.
    /// </summary>
    public abstract class CombatFacilityRuntime :
        BaseCampFacilityRuntime,
        IRaidFacilityRuntime,
        ICombatFacilityRuntime
    {
        protected readonly FacilityStatsRuntime StatsRuntime;

        private readonly BuildingHealthModule _healthModule;

        //────────────────────────────────────────────────────────────
        // IUnitRuntimeBase
        //────────────────────────────────────────────────────────────

        /// <summary>
        /// Все защитные сооружения принадлежат игроку.
        /// </summary>
        public virtual int TeamId => 0;
        
        public UnitGameplayDefinition RuntimeDefinition { get; }

        public BuildingHealthModule HealthModule => _healthModule;
        public IUnitStatsRuntime Stats => StatsRuntime;

        public ActiveEffectsRuntime Effects { get; }

        public bool IsInCombat { get; protected set; }

        public virtual Vector3 SpawnPosition => Vector3.zero;

        //────────────────────────────────────────────────────────────
        // Building API
        //────────────────────────────────────────────────────────────

        public sealed override bool CanReceiveDamage => true;

        public float CurrentHP => StatsRuntime.CurrentHP;

        public float MaxHP => StatsRuntime.MaxHP;

        public bool IsDestroyed => StatsRuntime.IsDead;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDestroyed;

        protected CombatFacilityRuntime(
            FacilityProxy proxy,
            FacilityModule config,
            BuildingHealthModule healthModule,
            GameTimeService timeService)
            : base(proxy, config, timeService)
        {

            //var campDefenseConfig = ServiceLocator.Current.Get<ConfigProvider>().Get<CampDefenseConfig>();
            _healthModule = healthModule;
            
            StatsRuntime = new FacilityStatsRuntime(
                Id,
                proxy,
                new Dictionary<StatId, float>
                {
                    { StatId.Health, healthModule.Settings.maxHealth }
                   // { StatId.Health , campDefenseConfig.CampHpDefault }  // берем хп штаба из конфига для этого режима
                },
                new EmptyEquipmentStatsProvider());

            Effects = new ActiveEffectsRuntime();

            StatsRuntime.OnStatChanged += HandleStatChanged;
            StatsRuntime.OnDeath += HandleDestroyed;
        }

        public override void Dispose()
        {
            StatsRuntime.OnStatChanged -= HandleStatChanged;
            StatsRuntime.OnDeath -= HandleDestroyed;
        }

        public virtual void Tick(float dt)
        {
            Effects.Tick(dt);
        }

        /// <summary>
        /// Починка сооружения.
        /// </summary>
        public virtual void Repair(float amount)
        {
            if (amount <= 0f)
                return;

            if (IsDestroyed)
                return;

            StatsRuntime.ModifyStat(StatId.Health, amount);
        }

        /// <summary>
        /// Полное восстановление HP.
        /// </summary>
        public virtual void RestoreFullHP()
        {
            StatsRuntime.Revive(MaxHP);
        }


        //────────────────────────────────────────────────────────────
        // Internal
        //────────────────────────────────────────────────────────────

        protected virtual void HandleDestroyed()
        {
            OnDestroyed?.Invoke();
        }


        private void HandleStatChanged(
            StatChangedEvent e,
            bool pushStart)
        {
            if (e.Type != StatId.Health)
                return;

            OnHealthChanged?.Invoke(e.Current, e.Max);

            MarkStateChanged();
        }
    }
}
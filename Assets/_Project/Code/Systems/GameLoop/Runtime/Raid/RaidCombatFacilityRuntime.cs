using System;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Code.Systems.Raid.Buildings;
using Galactic1.Game.Meta;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Runtime боевого сооружения,
    /// существующий только во время Camp Defense.
    ///
    /// Полностью независим от FacilityProxy.
    /// Любые изменения происходят только внутри snapshot.
    /// </summary>
    public partial class RaidCombatFacilityRuntime : 
        IRaidFacilityRuntime,
        IRetaliatingFacility
    {

        private readonly RaidFacilityStatsRuntime StatsRuntime;

        private readonly BuildingHealthModule _healthModule;



        public string Id { get; }
        public string ConfigId { get; }
        public FacilityModule Config { get; }
        public int TeamId => 0;
        
        public UnitGameplayDefinition RuntimeDefinition { get; }
        public IUnitStatsRuntime Stats => StatsRuntime;

        public BuildingHealthModule HealthModule => _healthModule;
        public ActiveEffectsRuntime Effects { get; }
        public bool IsInCombat { get; }




        public float CurrentHP => StatsRuntime.CurrentHP;
        public float MaxHP => StatsRuntime.MaxHP;
        public bool IsDestroyed => StatsRuntime.IsDead;


        public Vector3 SpawnPosition { get; }

        public FacilityType Type { get; }

        public int FacilityLimit { get; }
        public Vector2Int Position { get; }
        public int Rotation { get; }
        public int Level { get; }

        public virtual bool CanUpgrade => false;

        public virtual bool CanReceiveDamage => true;

        public event Action OnStateChanged;

        public event Action<Vector2Int> OnPositionChanged;

        public event Action<int> OnRotationChanged;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDestroyed;





        public RaidCombatFacilityRuntime(RaidFacilitySnapshot snapshot)
        {
            Type = snapshot.FacilityModule.FacilityType;
            Id = snapshot.FacilityId;
            ConfigId = snapshot.ConfigId;
            Config = snapshot.FacilityModule;
            Position = snapshot.Position;
            Rotation = snapshot.Rotation;
            Level = snapshot.Level;

            _healthModule = snapshot.HealthModule;

            //
            // У зданий нет экипировки,
            // поэтому используем EmptyEquipmentProvider.
            //
            StatsRuntime = new RaidFacilityStatsRuntime(
                snapshot.FacilityId,
                snapshot.StatsSnapshot.BaseStats,
                snapshot.StatsSnapshot.CurrentStats,
                new EmptyEquipmentStatsProvider());

            StatsRuntime.OnStatChanged += HandleStatChanged;
            StatsRuntime.OnDeath += HandleDestroyed;

        }

        public virtual void Dispose()
        {
            StatsRuntime.OnStatChanged -= HandleStatChanged;
            StatsRuntime.OnDeath -= HandleDestroyed;
        }


        protected virtual void MarkStateChanged()
        {
            OnStateChanged?.Invoke();
        }

        public void Tick(float dt)
        {

        }

        public virtual void Upgrade() {}

        public virtual FacilityUpgradeConfig GetUpgrade(int level)
        {
            return null;
        }

        public virtual void SetPosition(Vector2Int cell) {}

        public virtual void SetRotation(int rotation) {}
        
        
        
        public bool TryGetRetaliationDamage(out float damage)
        {
            if (Config.Item.HasModule<BuildingPassiveDamageModule>())
            {
                damage = Config.Item.BuildingPassiveDamage.EffectConfig.damagePerTick;
                return damage > 0f;
            }

            damage = 0f;
            return false;
        }


        private void HandleStatChanged(StatChangedEvent e, bool pushStart)
        {
            if (e.Type != StatId.Health)
                return;

            OnHealthChanged?.Invoke(e.Current, e.Max);
            MarkStateChanged();
        }

        /// <summary>
        /// Вызывается при уничтожении здания.
        /// </summary>
        protected void HandleDestroyed()
        {
            OnDestroyed?.Invoke();
        }

    }
}
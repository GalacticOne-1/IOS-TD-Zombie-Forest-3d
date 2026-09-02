using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units.Abstractions;
using R3;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Stats
{
    /// <summary>
    /// Базовый контроллер статов для любого юнита (игрок, NPC, питомец)
    /// </summary>
    public abstract class StatsRuntimeBase : IUnitStatsRuntime
    {
        public readonly IEquipmentStatsProvider EquipmentStatsProvider;
        
        public Dictionary<StatId, float> BaseStats { get; protected set; }            // неизменяемые
        public Dictionary<StatId, float> CalculatedStats { get; protected set; }      // результат пересчёта
        public Dictionary<StatId, float> CurrentStats { get; protected set; }         // runtime-состояние
        

        public event Action<StatChangedEvent, bool> OnStatChanged;
        public event Action OnDeath;
        
        public BuffController Buffs { get; protected set; }
        protected StatsRecalculator _recalculator;
        
        // Какие статы расходуемые (для CurrentStats)
        protected readonly HashSet<StatId> resourceStats = new()
        {
            StatId.Health,
            StatId.Hunger,
            StatId.Thirst
        };


        // === IUnitStatsRuntime ===
        public IReadOnlyDictionary<StatId, float> GetBaseStats => BaseStats;
        
        private readonly Dictionary<StatId, ReactiveProperty<float>> _reactiveStats = new();
        public IReadOnlyDictionary<StatId, ReactiveProperty<float>> CurrentStats_ => _reactiveStats;
        public float CurrentHP => CurrentStats[StatId.Health];
        public float MaxHP { get; private set; }
        private bool _isDead;
        public bool IsDead => _isDead;

        public string Owner { get; }
        
        
        // ----------------------------------------------------------
        // INITIALIZE
        // ----------------------------------------------------------
        public StatsRuntimeBase(
            string _owner,
            Dictionary<StatId, float> baseStats, 
            IEquipmentStatsProvider equipmentStatsProvider)
        {
            Owner = _owner;
            EquipmentStatsProvider = equipmentStatsProvider;

            BaseStats = baseStats;
            CalculatedStats  = new Dictionary<StatId, float>();
            CurrentStats = new Dictionary<StatId, float>();

            // Копия базовых значений (перед модификаторами)
            foreach (var kvp in BaseStats)
            {
                CalculatedStats[kvp.Key] = kvp.Value;
                CurrentStats[kvp.Key] = kvp.Value;
                
                // создаём ReactiveProperty для UI
                var reactive = new ReactiveProperty<float>(kvp.Value);
                _reactiveStats.Add(kvp.Key, reactive);
            }
            
            // максимальное хп = 100, нужно добавлять от предметов ??
            MaxHP = BaseStats[StatId.Health];

            Buffs = new BuffController(this);
            _recalculator = new StatsRecalculator(this);
        }

        protected virtual void ActivateLive()
        {
            // Пересчет max (CalculatedStats)
            _recalculator.Recalculate();
            EquipmentStatsProvider.OnUpdate += Recalculate;
            // Применяем сохранённые current-значения (HP/Hunger/Thirst)
            ApplySave();
            // ограничиваем под max
            ClampAllCurrentStats();
        }

        private void CheckDeath()
        {
            if (_isDead)
                return;

            if (CurrentStats[StatId.Health] <= 0)
            {
                _isDead = true;
                OnDeath?.Invoke();
            }
        }
        
        public void Revive(float hp)
        {
            _isDead = false;
            SetStat(StatId.Health, hp);
        }

        // ----------------------------------------------------------
        // RELOAD SAVE STATS (HP/Thirst/Hunger)
        // ----------------------------------------------------------
        protected virtual void ApplySave()
        {
            // Пустой — PlayerStatsController или EnemyStatsController переопределит
        }


        // ----------------------------------------------------------
        // MAIN API
        // ----------------------------------------------------------
        
        public void AddBuff(Buff buff) => Buffs.AddBuff(buff);
        public void RemoveBuff(BuffId buffId) => Buffs.RemoveBuff(buffId);
        public bool HasBuff(BuffId buffId) => Buffs.HasBuff(buffId);
        
        /// <summary>
        /// Использовать для снаряги и бафов
        /// </summary>
        public void Recalculate()
        {
            _recalculator.Recalculate();    // пересчет max (CalculatedStats)
            //ClampAllCurrentStats();         // если max изменились, чтобы CurrentStats не превышали новые max
            
            // Clamp только dirty resource stats
            foreach (var stat in _recalculator.DirtyStats)
            {
                CurrentStats[stat] = resourceStats.Contains(stat)
                    ? Mathf.Clamp(CurrentStats[stat], 0, CalculatedStats[stat])
                    : CalculatedStats[stat];
                _reactiveStats[stat].Value = CurrentStats[stat];
            }

            // Уведомляем ТОЛЬКО изменённые
            foreach (var stat in _recalculator.DirtyStats)
               NotifyStatChanged(stat);
            
            SyncProxyStats();               // пушим в Proxy → UI
        }
        
        protected virtual void SyncProxyStats() {}
        
        protected void SetIfExists(StatId stat, float value)
        {
            if (CurrentStats.ContainsKey(stat))
                CurrentStats[stat] = value;
        }

        
        
        public float GetMax(StatId stat) => CalculatedStats.ContainsKey(stat) ? CalculatedStats[stat] : 0;
        public float GetCurrent(StatId stat) => CurrentStats.ContainsKey(stat) ? CurrentStats[stat] : 0;


        /// <summary>
        /// Используется для ресурсных статов (health, hunger, thirst ...)
        /// <br/>То что может быть меньше макс. значения
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="amount"></param>
        public virtual void ModifyStat(StatId stat, float amount)
        {
            if (!CurrentStats.ContainsKey(stat))
                return;

            CurrentStats[stat] = Mathf.Clamp(CurrentStats[stat] + amount, 0, CalculatedStats[stat]);
            _reactiveStats[stat].Value = CurrentStats[stat];
            NotifyStatChanged(stat);
            
            if (stat == StatId.Health)
            {
                EventBus<HealthChangedEvent>.Raise(
                    new HealthChangedEvent(
                        Owner,
                        CurrentStats[StatId.Health],
                        GetMax(StatId.Health)));
                
                CheckDeath();
            }
        }
        public virtual void SetStat(StatId stat, float amount)
        {
            if (!CurrentStats.ContainsKey(stat))
                return;

            CurrentStats[stat] = Mathf.Clamp(amount, 0, CalculatedStats[stat]);
            _reactiveStats[stat].Value = CurrentStats[stat];
            NotifyStatChanged(stat);
            
            if (stat == StatId.Health)
            {
                EventBus<HealthChangedEvent>.Raise(
                    new HealthChangedEvent(
                        Owner,
                        CurrentStats[StatId.Health],
                        GetMax(StatId.Health)));
                
                CheckDeath();
            }
        }


        protected void ClampAllCurrentStats()
        {
            var list = CurrentStats.Keys.ToList();
            foreach (var stat in list)
            {
                CurrentStats[stat] = resourceStats.Contains(stat)
                    ? Mathf.Clamp(CurrentStats[stat], 0, CalculatedStats[stat])
                    : CalculatedStats[stat];
                _reactiveStats[stat].Value = CurrentStats[stat];
            }
        }
        
        public void PushAllStats()
        {
            foreach (var stat in CurrentStats.Keys)
                NotifyStatChanged(stat, true);
        }
        
        protected void NotifyStatChanged(StatId type, bool pushStart = false)
        {
            OnStatChanged?.Invoke(new StatChangedEvent
                {
                    Type = type,
                    Current = GetCurrent(type),
                    Max = GetMax(type)
                },
                pushStart);
        }


        // protected virtual void Update()
        // {
        //     Buffs.Update(Time.deltaTime);
        // }
    }
}

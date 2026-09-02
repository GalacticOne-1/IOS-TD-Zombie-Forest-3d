using System;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Runtime.Production;
using UnityEngine;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Базовый runtime любого здания лагеря.
    /// Не содержит бизнес-логики конкретного типа.
    /// </summary>
    public abstract class BaseCampFacilityRuntime : IDisposable, IFacilityRuntime
    {
        protected readonly FacilityProxy Proxy;
        protected readonly GameTimeService TimeService;
        public FacilityModule Config {get; }
        public abstract FacilityType Type { get; }

        
        
        public string Id => Proxy.UniqueId;
        public string ConfigId => Proxy.ConfigId;
        public Vector2Int Position => new((int)Proxy.PosX.Value, (int)Proxy.PosZ.Value);
        public int Rotation => Proxy.Rotation.Value;
        
        /// <summary>
        /// Апгрейд доступен только если здание upgradeable и уровень не достиг максимума.
        /// </summary>
        public virtual bool CanUpgrade => Config.IsUpgradeable && Level < Config.MaxLevel;
        public virtual bool CanReceiveDamage => false;
        public int Level => Proxy.Level.Value;
        public int TotalWorldHour => TimeService.TotalWorldHours;


        public int FacilityLimit { get; } = 2;
        
        

        /// <summary>
        /// Вызывается при любом изменении состояния:
        /// • добавление задания
        /// • завершение
        /// • сбор
        /// • skip
        /// • продвижен
        public event Action OnStateChanged;
        public event Action<Vector2Int> OnPositionChanged;
        public event Action<int> OnRotationChanged;
        
        

        protected BaseCampFacilityRuntime(
            FacilityProxy proxy,
            FacilityModule config,
            GameTimeService timeService)
        {
            Proxy = proxy;
            TimeService = timeService;
            Config = config;
        }

        public abstract void Dispose();
        
        
        /// <summary>
        /// Фиксирует изменения в Proxy и уведомляет подписчиков.
        /// Единственная точка сохранения состояния.
        /// </summary>
        protected void MarkStateChanged() => OnStateChanged?.Invoke();


        public FacilityUpgradeConfig GetUpgrade(int toLevel)
            => Config.GetUpgrade(toLevel);
        
        public void Upgrade()
        {
            if (Level >= Config.MaxLevel)
                return;

            Proxy.Level.Value++;
            MarkStateChanged();
        }
        
        /// <summary>
        /// Единственное место где меняется позиция здания
        /// </summary>
        public void SetPosition(Vector2Int cell)
        {
            Proxy.PosX.Value = cell.x;
            Proxy.PosZ.Value = cell.y;

            OnPositionChanged?.Invoke(cell);
            MarkStateChanged();
        }
        /// <summary>
        /// Единственное место где повораяиваем здание
        /// </summary>
        public void SetRotation(int rotation)
        {
            Proxy.Rotation.Value = rotation;
            
            OnRotationChanged?.Invoke(rotation);
            MarkStateChanged();
        }
        
        public virtual void TakeDamage(float damage)
        {
            // no-op по умолчанию — переопределяется только зданиями, способными получать урон
        }
    }
}
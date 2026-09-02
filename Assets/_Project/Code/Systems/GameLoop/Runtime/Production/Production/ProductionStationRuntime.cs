
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Game.Runtime.Production
{
    /// <summary>
    /// Runtime-логика производственного здания.
    /// 
    /// • Управляет очередью (max 5)
    /// • Обрабатывает время через GameTimeService
    /// • Является single source of truth
    /// • Не знает о Scene / UI
    /// • Генерирует события изменений состояния
    /// </summary>
    public sealed class ProductionStationRuntime : BaseProductionStationRuntime
    {
        public override FacilityType Type { get; } = FacilityType.Production;
        //public override bool CanUpgrade => false;

        protected override int MaxSlots { get; } = 5;
        
        public ProductionStationRuntime(
            FacilityProxy proxy, 
            FacilityModule config, 
            GameTimeService timeService) 
            : base(proxy, config, timeService)
        {
        }
        

    }
}
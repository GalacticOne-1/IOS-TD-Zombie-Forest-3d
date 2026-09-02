
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Game.Runtime.Production
{
    /// <summary>
    /// Runtime-логика Recycler станции.
    ///
    /// • Управляет очередью (max 5)
    /// • Поддерживает multi-output jobs
    /// • Работает через GameTimeService
    /// • Single source of truth
    /// • Не знает об инвентаре / UI / Scene
    /// </summary>
    public sealed class RecyclerStationRuntime : BaseProductionStationRuntime
    {
        public override FacilityType Type { get; } = FacilityType.Recycler;
        public override bool CanUpgrade => false;
        protected override int MaxSlots { get; } = 5;

        public RecyclerStationRuntime(
            FacilityProxy proxy, 
            FacilityModule config, 
            GameTimeService timeService) 
            : base(proxy, config, timeService)
        {
        }
    }
}
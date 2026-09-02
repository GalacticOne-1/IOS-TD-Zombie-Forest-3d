using Galactic1.Code.Gameplay.Construction.Repair;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Runtime.Building
{
    public sealed class WallFacilityRuntime : 
        CombatFacilityRuntime,
        IRepairableFacility
    {
        private readonly DefenseFacilityModule _config;

        public override FacilityType Type => FacilityType.Defense;

        public WallFacilityRuntime(
            FacilityProxy proxy,
            DefenseFacilityModule config,
            GameTimeService timeService)
            : base(
                proxy,
                config,
                config.Item.GetModule<BuildingHealthModule>(),
                timeService)
        {
            _config = config;
        }

        protected override void HandleDestroyed()
        {
            DLog.Alert("WallFacility destroyed");
        }
    }
}
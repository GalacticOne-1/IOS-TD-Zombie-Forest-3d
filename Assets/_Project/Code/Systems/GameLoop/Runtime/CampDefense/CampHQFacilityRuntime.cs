
using Galactic1.Code.Systems.GameTime;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Runtime главного здания лагеря (Main Base).
    /// Основная цель орды во время Camp Defense.
    ///
    /// Получает урон через общий боевой pipeline (IUnitRuntimeBase/Stats),
    /// аналогично EnemyRuntime — здание "приведено" к юниту только для
    /// целей боевой системы, никакой Camp Defense логики здесь нет.
    /// </summary>
    public sealed class CampHQFacilityRuntime : CombatFacilityRuntime
    {
        private readonly CampHQModule _config;

        public override FacilityType Type => FacilityType.CampHQ;

        public override bool CanUpgrade => false;

        public CampHQFacilityRuntime(
            FacilityProxy proxy,
            CampHQModule config,
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
            // TODO:
            // MissionFailed
            // CampDestroyed
            // Penalty
        }
    }
}
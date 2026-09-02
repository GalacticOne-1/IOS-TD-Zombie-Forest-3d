using Galactic1.Code.Systems.GameTime;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Жилой модуль. Увеличивает лимит юнитов лагеря.
    /// </summary>
    public sealed class LivingModuleFacilityRuntime :
        BaseCampFacilityRuntime,
        ILivingModuleFacilityRuntime
    {
        private readonly ICampCapacityService _capacity;

        public override FacilityType Type => FacilityType.LivingModule;
        public override bool CanUpgrade => false;

        public LivingModuleFacilityRuntime(
            FacilityProxy proxy,
            LivingModule config,
            GameTimeService timeService,
            ICampCapacityService capacity)
            : base(proxy, config, timeService)
        {
            _capacity = capacity;
        }
        
        public override void Dispose(){}
    }
}
using System;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Core.Systems.GameLoopSession;

namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Рассчитывает лимит юнитов на основе жилых модулей.
    /// </summary>
    public sealed class CampCapacityService : ICampCapacityService
    {
        private readonly GameLoopContext _gameLoopContext;
        private readonly IFacilityRuntimeService _facilities;

        
        private const int BaseCapacity = 2;
        private const int LivingModuleBonus = 2;


        public event Action OnCapacityChanged;
        
        
        

        public CampCapacityService(
            GameLoopContext gameLoopContext, 
            IFacilityRuntimeService facilities)
        {
            _gameLoopContext = gameLoopContext;
            _facilities = facilities;
            
            _gameLoopContext.OnBuildingChanged += NotifyChanged;

        }

        public int GetMaxCapacity()
        {
            int modules = _gameLoopContext.GetFacilityCount(FacilityType.LivingModule);

            return BaseCapacity + modules * LivingModuleBonus;
        }

        public int GetCurrentUnits()
            => ServiceLocator.Current.Get<GameSession>().GameLoopContext.PlayerUnits.Count;

        public bool HasFreeSlot() => GetCurrentUnits() < GetMaxCapacity();

        public void NotifyChanged() => OnCapacityChanged?.Invoke();
    }
}
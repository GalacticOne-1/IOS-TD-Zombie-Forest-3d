using System;
using Galactic1.Game.Runtime.Production;

namespace Galactic1.Code.Systems.Runtime.Building
{
    public class LivingModuleSceneAdapter : IFacilitySceneAdapter
    {
        private readonly ILivingModuleFacilityRuntime _runtime;
        
        
        public FacilityType Type => _runtime.Type;
        public event Action OnStateChanged;

        public LivingModuleSceneAdapter(ILivingModuleFacilityRuntime runtime)
        {
            _runtime = runtime;
        }
    }
}
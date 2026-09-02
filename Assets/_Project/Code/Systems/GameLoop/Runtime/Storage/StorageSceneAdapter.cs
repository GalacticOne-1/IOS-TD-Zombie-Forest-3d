using System;

namespace Galactic1.Code.Systems.Runtime.Building
{
    public class StorageSceneAdapter : IFacilitySceneAdapter
    {
        private readonly IStorageFacilityRuntime _runtime;
        
        
        public FacilityType Type => _runtime.Type;
        public event Action OnStateChanged;

        public StorageSceneAdapter(IStorageFacilityRuntime runtime)
        {
            _runtime = runtime;
        }
    }
}
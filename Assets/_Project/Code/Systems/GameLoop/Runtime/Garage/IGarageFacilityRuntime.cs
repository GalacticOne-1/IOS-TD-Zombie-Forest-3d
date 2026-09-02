using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Runtime.Building
{
    public interface IGarageFacilityRuntime
    {
        FacilityType Type { get; }
        event Action OnStateChanged;
        RuntimeId GetCurrentVehicleId();
        IReadOnlyCollection<RuntimeId> GetUnlockedModules();
        bool IsModuleUnlocked(RuntimeId moduleId);
        void UnlockModule(RuntimeId moduleId);
        void ReplaceCurrentVehicle(RuntimeId configId);
    }
}
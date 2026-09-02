using System;

namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Отвечает за расчёт вместимости лагеря.
    /// Источник истины — Runtime.
    /// </summary>
    public interface ICampCapacityService : IGameService
    {
        event Action OnCapacityChanged;

        
        
        int GetMaxCapacity();
        int GetCurrentUnits();
        bool HasFreeSlot();
        
        void NotifyChanged();
    }
}
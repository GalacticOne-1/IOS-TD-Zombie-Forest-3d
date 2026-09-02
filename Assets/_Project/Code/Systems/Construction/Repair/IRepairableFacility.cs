using System;

namespace Galactic1.Code.Gameplay.Construction.Repair
{
    /// <summary>
    /// Контракт runtime-объекта, поддерживающего систему ремонта.
    /// Позволяет repair-пайплайну не зависеть от конкретного CombatFacilityRuntime.
    /// </summary>
    public interface IRepairableFacility
    {
        float CurrentHP { get; }
        float MaxHP { get; }
        bool IsDestroyed { get; }

        event Action<float, float> OnHealthChanged;

        void RestoreFullHP();
    }
}
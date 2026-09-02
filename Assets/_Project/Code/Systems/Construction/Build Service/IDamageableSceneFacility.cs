using System;

namespace Galactic1.Code.AbstractFactory
{
    /// <summary>
    /// Реализуется адаптерами боевых сооружений (CombatFacilitySceneAdapter).
    /// Даёт Scene-слою доступ к HP без прямой ссылки на CombatFacilityRuntime.
    /// </summary>
    public interface IDamageableSceneFacility
    {
        float CurrentHP { get; }
        float MaxHP { get; }

        event Action<float, float> OnHealthChanged;
        event Action OnDestroyed;
    }
}

namespace Galactic1.Code.Gameplay.Combat.Events
{
    public readonly struct HealthChangedEvent : IEvent
    {
        public readonly string UnitId;
        public readonly float CurrentHealth;
        public readonly float MaxHealth;

        public HealthChangedEvent(string unitId, float currentHealth, float maxHealth)
        {
            UnitId = unitId;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }
}
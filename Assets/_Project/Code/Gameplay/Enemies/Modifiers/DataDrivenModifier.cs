using Galactic1.Code.Gameplay.Enemies.Spawning;

namespace Galactic1.Code.Gameplay.Enemies.Modifiers
{
    /// <summary>
    /// Универсальная data-driven реализация IEnemyModifier.
    /// Читает параметры из EnemyModifierDefinition.
    /// Не требует отдельного класса для каждого модификатора.
    /// </summary>
    internal sealed class DataDrivenModifier : IEnemyModifier
    {
        private readonly EnemyModifierDefinition _definition;

        public string ModifierId => _definition.ModifierId;

        public DataDrivenModifier(EnemyModifierDefinition definition)
        {
            _definition = definition;
        }

        public void Apply(EnemySpawnContext context)
        {
            var mutation = context.MutationContext;
            if (mutation == null) return;

            // Применяем множители статов
            foreach (var entry in _definition.StatMultipliers)
                mutation.MultiplyStatIfExists(entry.StatId, entry.Multiplier);

            // Движение
            if (_definition.WalkSpeedMultiplier != 1f || _definition.RunSpeedMultiplier != 1f)
            {
                mutation.Movement ??= new MovementOverride();
                mutation.Movement.WalkSpeedMultiplier =
                    (mutation.Movement.WalkSpeedMultiplier ?? 1f) * _definition.WalkSpeedMultiplier;
                mutation.Movement.RunSpeedMultiplier =
                    (mutation.Movement.RunSpeedMultiplier ?? 1f) * _definition.RunSpeedMultiplier;
            }

            // Флаг элиты
            if (_definition.SetsEliteFlag)
                mutation.IsElite = true;

            // ThreatLevel
            mutation.ThreatMultiplier += _definition.ThreatMultiplierBonus;
        }
    }
}
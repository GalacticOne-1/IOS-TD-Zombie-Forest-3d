
using Galactic1.Code.Gameplay.Enemies.Spawning;

namespace Galactic1.Code.Gameplay.Enemies.Modifiers
{
    /// <summary>
    /// Контракт геймплейной мутации врага.
    ///
    /// Модификатор работает ТОЛЬКО с EnemyStatMutationContext — изменяемым
    /// промежуточным объектом, живущим до создания EnemyRuntimeDefinition.
    ///
    /// EnemyRuntimeDefinition после создания иммутабелен.
    /// Модификаторы НЕ должны к нему обращаться.
    ///
    /// Примеры реализаций:
    ///   ArmorModifier   — +50% хп, -20% скорости
    ///   ToxicModifier   — визуальный prefab оверрайд, +DoT флаг
    ///   EliteModifier   — x1.5 все статы, IsElite = true
    /// </summary>
    public interface IEnemyModifier
    {
        /// <summary>Уникальный строковый ID. Используется EnemyModifierDatabase для резолюции.</summary>
        string ModifierId { get; }

        /// <summary>
        /// Применяет мутацию к SpawnContext.
        /// Читает и изменяет context.MutationContext.
        ///
        /// НЕ должен:
        ///   — создавать сцен-объекты
        ///   — обращаться к фабрикам
        ///   — читать или изменять context.RuntimeDefinition
        /// </summary>
        void Apply(EnemySpawnContext context);
    }
}
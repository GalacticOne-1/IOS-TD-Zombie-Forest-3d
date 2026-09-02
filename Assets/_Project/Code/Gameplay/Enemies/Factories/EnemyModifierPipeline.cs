
using Galactic1.Code.Gameplay.Enemies.Spawning;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Modifiers
{
    /// <summary>
    /// Пайплайн применения геймплейных мутаций.
    ///
    /// Работает ТОЛЬКО с context.MutationContext — изменяемым промежуточным объектом.
    /// НЕ касается context.RuntimeDefinition.
    ///
    /// Правильный порядок вызовов в EnemySpawnPipeline:
    ///   1. Resolver → context.MutationContext создан
    ///   2. ModifierPipeline.Apply(context) ← этот класс
    ///   3. DefinitionBuilder.Build(context.MutationContext) → иммутабельная Definition
    /// </summary>
    public sealed class EnemyModifierPipeline
    {
        private readonly EnemyModifierDatabase _database;

        public EnemyModifierPipeline(EnemyModifierDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Резолвит и применяет модификаторы к context.MutationContext.
        /// Заполняет context.AppliedModifiers для дебага.
        /// </summary>
        public void Apply(EnemySpawnContext context)
        {
            if (context.Request.ModifierIds == null || context.Request.ModifierIds.Count == 0)
                return;

            if (context.MutationContext == null)
            {
                Debug.LogError(
                    "[EnemyModifierPipeline] MutationContext == null. " +
                    "Убедись что EnemySpawnPipeline создал MutationContext до вызова Apply.");
                return;
            }

            var modifiers = _database.Resolve(context.Request.ModifierIds);
            context.AppliedModifiers = modifiers;

            foreach (var modifier in modifiers)
            {
#if UNITY_EDITOR
                Debug.Log($"[ModifierPipeline] '{modifier.ModifierId}' → {context.Request.EnemyId}");
#endif
                modifier.Apply(context);
            }
        }
    }
}
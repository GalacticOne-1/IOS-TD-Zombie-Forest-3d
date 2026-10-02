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
    /// Порядок применения (детерминирован):
    ///   1. Явные модификаторы из Request.ModifierIds — в порядке списка.
    ///   2. Глобальный прогрессионный шаг — ВСЕГДА последний и выполняется
    ///      даже если ModifierIds пуст/null.
    ///
    /// Прогрессионный шаг применяется к уже модифицированным статам:
    ///   Damage 20 × Elite 1.5 × Progression 0.5 = 15.
    ///
    /// Правильный порядок вызовов в EnemySpawnPipeline:
    ///   1. MutationContext создан
    ///   2. ModifierPipeline.Apply(context) ← этот класс
    ///   3. DefinitionBuilder.Build(context.MutationContext) → иммутабельная Definition
    /// </summary>
    public sealed class EnemyModifierPipeline
    {
        private readonly EnemyModifierDatabase _database;
        private readonly IEnemyModifier _progressionScaling;

        public EnemyModifierPipeline(
            EnemyModifierDatabase database,
            IEnemyModifier progressionScaling = null)
        {
            _database = database;
            _progressionScaling = progressionScaling;
        }

        /// <summary>
        /// Резолвит и применяет явные модификаторы, затем глобальный прогрессионный шаг.
        /// Заполняет context.AppliedModifiers (только явные) для дебага.
        /// </summary>
        public void Apply(EnemySpawnContext context)
        {
            if (context.MutationContext == null)
            {
                Debug.LogError(
                    "[EnemyModifierPipeline] MutationContext == null. " +
                    "Убедись что EnemySpawnPipeline создал MutationContext до вызова Apply.");
                return;
            }

            // 1. Явные модификаторы
            var ids = context.Request.ModifierIds;
            if (ids != null && ids.Count > 0)
            {
                var modifiers = _database.Resolve(ids);
                context.AppliedModifiers = modifiers;

                foreach (var modifier in modifiers)
                {
#if UNITY_EDITOR
                    Debug.Log($"[ModifierPipeline] '{modifier.ModifierId}' → {context.Request.EnemyId}");
#endif
                    modifier.Apply(context);
                }
            }

            // 2. Глобальный шаг (всегда последним, вне раннего выхода)
            _progressionScaling?.Apply(context);
        }
    }
}

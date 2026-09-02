
using System.Collections.Generic;
using Galactic1.Code.Systems.Raid.Enemies;

namespace Galactic1.Code.Gameplay.Enemies.Modifiers
{
    /// <summary>
    /// Изменяемый промежуточный контекст для модификаторов.
    ///
    /// Живёт только внутри EnemySpawnPipeline во время применения модификаторов.
    /// После сборки EnemyRuntimeDefinition уничтожается.
    ///
    /// Правильный поток:
    ///   базовые статы (из EnemyStatsFactory)
    ///     ↓
    ///   EnemyModifierPipeline.Apply(mutationContext) — мутирует этот объект
    ///     ↓
    ///   EnemyRuntimeDefinitionBuilder.Build(mutationContext) — читает финальные данные
    ///     ↓
    ///   EnemyRuntimeDefinition (иммутабельный)
    ///
    /// ПРАВИЛО: модификаторы мутируют ТОЛЬКО этот объект.
    /// EnemyRuntimeDefinition после создания никогда не изменяется.
    /// </summary>
    public sealed class EnemyStatMutationContext
    {
        /// <summary>
        /// Изменяемый словарь статов. Модификаторы пишут сюда напрямую.
        /// После передачи в Builder становится иммутабельным снапшотом.
        /// </summary>
        public Dictionary<StatId, float> Stats { get; }

        /// <summary>Флаг элиты. Устанавливается EliteModifier.</summary>
        public bool IsElite { get; set; }

        /// <summary>
        /// Множитель угрозы. Накапливается модификаторами.
        /// Итоговый ThreatLevel = base * ThreatMultiplier.
        /// </summary>
        public float ThreatMultiplier { get; set; } = 1f;

        /// <summary>
        /// Изменяемое визуальное представление.
        /// Модификаторы могут заменить Presentation целиком или отдельные поля
        /// (например ToxicModifier подставляет токсичный prefab).
        /// </summary>
        public EnemyPresentationDefinition Presentation { get; set; }

        /// <summary>
        /// Изменяемые параметры движения.
        /// ArmorModifier может снизить скорость, FreezeModifier — заморозить.
        /// null = использовать данные из archetype config без изменений.
        /// </summary>
        public MovementOverride Movement { get; set; }

        public EnemyStatMutationContext(
            Dictionary<StatId, float> stats,
            EnemyPresentationDefinition presentation)
        {
            Stats = stats;
            Presentation = presentation;
        }

        // ── Вспомогательные методы ────────────────────────────────────

        /// <summary>Умножает стат на множитель. Безопасен если стат отсутствует.</summary>
        public void MultiplyStatIfExists(StatId statId, float multiplier)
        {
            if (Stats.ContainsKey(statId))
                Stats[statId] *= multiplier;
        }

        /// <summary>Добавляет значение к стату. Создаёт стат если его не было.</summary>
        public void AddStat(StatId statId, float value)
        {
            if (Stats.ContainsKey(statId))
                Stats[statId] += value;
            else
                Stats[statId] = value;
        }
    }

    /// <summary>
    /// Оверрайд параметров движения, применяемый модификатором.
    /// null-поля означают "оставить значение из archetype config".
    /// </summary>
    public sealed class MovementOverride
    {
        public float? WalkSpeedMultiplier;
        public float? RunSpeedMultiplier;
    }
}
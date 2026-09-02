
using System.Collections.Generic;

namespace Galactic1.Code.Gameplay.Enemies.Stats
{
    /// <summary>
    /// Иммутабельный снапшот финальных статов врага.
    ///
    /// Создаётся ОДИН РАЗ в EnemySpawnPipeline ПОСЛЕ применения всех модификаторов.
    /// После создания НЕ изменяется никогда.
    ///
    /// ПРАВИЛО: никакой внешний код не может мутировать Stats напрямую.
    ///   Для чтения — используй Get() или TryGet().
    ///   Для модификации — работай с EnemyStatMutationContext ДО создания снапшота.
    ///
    /// Замена старого EnemyStatsSnapshot с публичным mutable Dictionary.
    /// </summary>
    public sealed class EnemyStatsSnapshot
    {
        private readonly IReadOnlyDictionary<StatId, float> _stats;

        /// <summary>
        /// Иммутабельное представление статов.
        /// Доступно только для чтения.
        /// </summary>
        public IReadOnlyDictionary<StatId, float> Stats => _stats;

        /// <summary>
        /// Создаёт снапшот из финального словаря статов.
        /// Словарь копируется — оригинал больше не влияет на снапшот.
        /// </summary>
        public EnemyStatsSnapshot(Dictionary<StatId, float> finalStats)
        {
            // Копируем — снапшот не зависит от исходного словаря
            _stats = new Dictionary<StatId, float>(finalStats);
        }

        // ── Методы доступа ────────────────────────────────────────────

        /// <summary>
        /// Возвращает значение стата. Кидает KeyNotFoundException если стата нет.
        /// Используй TryGet если не уверен в наличии стата.
        /// </summary>
        public float Get(StatId statId) => _stats[statId];

        /// <summary>
        /// Безопасный доступ к стату.
        /// Возвращает defaultValue если стат отсутствует.
        /// </summary>
        public float GetOrDefault(StatId statId, float defaultValue = 0f) =>
            _stats.TryGetValue(statId, out var value) ? value : defaultValue;

        /// <summary>Возвращает true если стат присутствует в снапшоте.</summary>
        public bool TryGet(StatId statId, out float value) =>
            _stats.TryGetValue(statId, out value);

        /// <summary>Удобный доступ через индексатор.</summary>
        public float this[StatId statId] => Get(statId);
    }
}
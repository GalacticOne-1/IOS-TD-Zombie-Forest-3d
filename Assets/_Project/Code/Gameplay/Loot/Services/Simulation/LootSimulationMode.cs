namespace Galactic1.RaidLoot.Diagnostics
{
    public enum LootSimulationMode
    {
        /// <summary>
        /// Один seed → воспроизводимый результат.
        /// Используется для дебага и анализа Trace.
        /// </summary>
        Deterministic,

        /// <summary>
        /// Уникальный seed для каждой итерации → реальное распределение вероятностей.
        /// Используется для балансировки таблиц и мультипликаторов.
        /// </summary>
        Statistical,
    }
}
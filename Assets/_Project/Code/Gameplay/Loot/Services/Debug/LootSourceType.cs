namespace Galactic1.RaidLoot.Enums
{
    /// <summary>
    /// Источник происхождения записи лута.
    /// Используется для трассировки и отладки экономики.
    /// </summary>
    public enum LootSourceType
    {
        /// <summary>Случайный лут из слота контейнера.</summary>
        Container,

        /// <summary>Гарантированный лут из guaranteed-слоя контейнера.</summary>
        ContainerGuaranteed,

        /// <summary>Гарантированный лут уровня локации (food, water, fuel и т.п.).</summary>
        LocationGuaranteed,
    }
}
namespace Galactic1.Code.Systems.Interaction
{
    /// <summary>
    /// Runtime-политика доступности взаимодействий.
    ///
    /// Не содержит логики.
    /// Является единственным источником истины о том,
    /// какие типы взаимодействий сейчас разрешены.
    ///
    /// Изменяется сценариями, кат-сценами,
    /// обучением и другими игровыми системами.
    /// </summary>
    public sealed class InteractionPolicy
    {
        /// <summary>
        /// Разрешено взаимодействие со зданиями лагеря.
        /// </summary>
        public bool CanInteractWithFacilities { get; set; } = true;

        /// <summary>
        /// Разрешено открывать контейнеры и подбирать лут.
        /// </summary>
        public bool CanInteractWithLoot { get; set; } = true;

        /// <summary>
        /// Разрешено взаимодействие с NPC.
        /// </summary>
        public bool CanInteractWithNpc { get; set; } = true;

        /// <summary>
        /// Разрешено взаимодействие с транспортом.
        /// </summary>
        public bool CanInteractWithVehicles { get; set; } = true;

        /// <summary>
        /// Разрешено взаимодействие с интерактивными объектами
        /// (рычаги, терминалы и т.п.).
        /// </summary>
        public bool CanInteractWithWorldObjects { get; set; } = true;

        /// <summary>
        /// Возвращает все взаимодействия в состояние по умолчанию.
        /// </summary>
        public void Reset()
        {
            CanInteractWithFacilities = true;
            CanInteractWithLoot = true;
            CanInteractWithNpc = true;
            CanInteractWithVehicles = true;
            CanInteractWithWorldObjects = true;
        }

        /// <summary>
        /// Полностью запрещает любые взаимодействия.
        /// Не влияет на управление камерой и перемещение.
        /// </summary>
        public void DisableAll()
        {
            CanInteractWithFacilities = false;
            CanInteractWithLoot = false;
            CanInteractWithNpc = false;
            CanInteractWithVehicles = false;
            CanInteractWithWorldObjects = false;
        }
    }
}
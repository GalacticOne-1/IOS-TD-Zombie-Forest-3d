using System.Collections.Generic;
using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Базовый runtime-класс сущности,
    /// которая владеет одним или несколькими контейнерами инвентаря.
    /// Не содержит геймплейной логики — только инфраструктуру инвентаря.
    /// </summary>
    public abstract class InventoryOwnerRuntime
    {

        /// <summary>
        /// Все источники инвентаря, принадлежащие сущности.
        /// </summary>
        protected readonly List<IInventorySource> _sources = new();

        public IReadOnlyList<IInventorySource> Sources => _sources;

        /// <summary>
        /// Регистрация контейнера как источника.
        /// Вызывается наследниками при инициализации.
        /// </summary>
        protected void RegisterInventorySource(IInventorySource source)
        {
            _sources.Add(source);
        }
    }
}
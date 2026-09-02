using Galactic1;
using Galactic1.Core;
using UnityEngine;

namespace Galactic1.Gameplay.Locations
{
    /// <summary>
    /// Интерфейс режима загрузки локации (Camp / Regular / Event).
    /// Реализации инкапсулируют логику инициализации конкретного типа локации.
    /// </summary>
    public interface ILocationLoaderMode
    {
        /// <summary>Запустить загрузку локации</summary>
        void Load(LocationContext ctx, DIContainer container);

        /// <summary>Асинхронная версия если нужно</summary>
        // Task LoadAsync(LocationContext ctx, DIContainer container);
    }
}
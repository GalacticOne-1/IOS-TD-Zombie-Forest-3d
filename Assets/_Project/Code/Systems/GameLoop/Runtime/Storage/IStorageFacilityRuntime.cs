using System.Collections.Generic;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Runtime интерфейс здания-хранилища.
    /// 
    /// Хранилище НЕ имеет собственного инвентаря.
    /// Оно только разрешает автосбор предметов определённых ItemTag
    /// в общий инвентарь игрока.
    /// </summary>
    public interface IStorageFacilityRuntime
    {
        FacilityType Type { get; }
        StorageModule Module { get; }
        /// <summary>
        /// Теги предметов, для которых разрешён автосбор
        /// </summary>
        IReadOnlyList<ItemTag> SupportedTags { get; }

        /// <summary>
        /// Проверка поддерживается ли тег
        /// </summary>
        bool Supports(ItemTag tag);
    }
}
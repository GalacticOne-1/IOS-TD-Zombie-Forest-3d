using System.Collections.Generic;
using Galactic1.Core.Enums;

namespace Gameplay.Gameplay.Interaction
{
    /// <summary>
    /// Ресурс сообщает, какой инструмент ему требуется.
    /// </summary>
    public interface IToolRequirement
    {
        List<ItemEquipType> RequiredToolType { get; }
    }
}
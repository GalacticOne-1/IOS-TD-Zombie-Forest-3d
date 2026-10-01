using System;
using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>Подсветка ПЕРВОГО пустого слота в инвентаре указанного типа — для заданий
    /// вида "перенеси предмет из A в B": сначала TutorialInventoryItemQuery подсвечивает
    /// сам предмет, затем (следующий guidance-entry, после начала драга/клика) этот query
    /// подсвечивает, КУДА его класть.</summary>
    [Serializable]
    public sealed class TutorialEmptySlotQuery : TutorialTargetQuery
    {
        public InventorySourceType sourceType;

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            error = null;
            return true; // InventorySourceType — обычный enum, невалидного значения не бывает
        }
#endif
    }
}
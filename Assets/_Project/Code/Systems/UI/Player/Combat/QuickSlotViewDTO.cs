using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.UI.UnitCard
{
    public struct QuickSlotViewDTO
    {
        public Sprite Icon;
        public int Count;
        public bool HasItem;
        public List<int> SourceSlotIndices;
    }
}
using Galactic1.Code.Gameplay.Grid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction
{
    public static class GridZoneColors
    {
        public static Color Get(GridZoneTag tag)
        {
            return tag switch
            {
                GridZoneTag.Locked => Color.gray,
                GridZoneTag.Main => Color.blue,
                GridZoneTag.Defense => Color.orange,
                _ => Color.white
            };
        }
    }
}
using System;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Grid
{
    /// <summary>
    /// Прямоугольная статическая заблокированная область сетки.
    /// Origin — нижняя левая клетка прямоугольника.
    /// </summary>
    [Serializable]
    public class BlockedGridArea
    {
        public Vector2Int Origin;
        public int Width;
        public int Height;
        public GridZoneTag Tag = GridZoneTag.None;
    }
}
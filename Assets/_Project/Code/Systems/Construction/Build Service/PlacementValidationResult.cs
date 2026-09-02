using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Результат проверки размещения.
    /// Содержит итог, клетки footprint и сообщение для UI.
    /// </summary>
    public class PlacementValidationResult
    {
        public bool IsValid;
        public string Message;
        public List<Vector2Int> Cells;

        public static PlacementValidationResult Pass(List<Vector2Int> cells)
        {
            return new PlacementValidationResult
            {
                IsValid = true,
                Message = "",
                Cells = cells
            };
        }

        public static PlacementValidationResult Fail(string message, List<Vector2Int> cells)
        {
            return new PlacementValidationResult
            {
                IsValid = false,
                Message = message,
                Cells = cells
            };
        }
    }
}
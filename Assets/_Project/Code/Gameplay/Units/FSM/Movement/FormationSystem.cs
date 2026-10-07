using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Вычисляет локальные оффсеты для каждого юнита в отряде.
    /// desiredPosition = SquadCenter + GetOffset(index)
    ///
    /// Итоговое центрирование (центроид оффсетов = 0) делает
    /// SquadFormationSlots.RebuildOffsets — для любого типа формации.
    /// </summary>
    public static class FormationSystem
    {
        public enum FormationType
        {
            Wedge,
            Line,
            Circle,
            Grid
        }

        public static Vector3 GetOffset(
            int unitIndex,
            int totalUnits,
            FormationType formation,
            Vector3 moveDirection,
            GridParams gridParams = default)
        {
            return formation switch
            {
                FormationType.Wedge => WedgeOffset(unitIndex, totalUnits, moveDirection),
                FormationType.Line => LineOffset(unitIndex, totalUnits, moveDirection),
                FormationType.Circle => CircleOffset(unitIndex, totalUnits),
                FormationType.Grid => GridOffset(unitIndex, totalUnits, moveDirection,
                    gridParams.UnitsPerRow,
                    gridParams.SpacingX,
                    gridParams.SpacingZ),
                _ => Vector3.zero
            };
        }

        // ─── Wedge (клин) ───────────────────────────────────────────
        private static Vector3 WedgeOffset(int i, int total, Vector3 dir)
        {
            if (i == 0) return Vector3.zero;

            var right = Vector3.Cross(Vector3.up, dir).normalized;
            int row = (i + 1) / 2;
            int side = (i % 2 == 1) ? -1 : 1;

            const float rowSpacing = 1.5f;
            const float sideSpacing = 1.5f;

            return -dir * (row * rowSpacing) + right * (side * row * sideSpacing);
        }

        // ─── Line (шеренга) ─────────────────────────────────────────
        private static Vector3 LineOffset(int i, int total, Vector3 dir)
        {
            var right = Vector3.Cross(Vector3.up, dir).normalized;
            float center = (total - 1) * 0.5f;
            return right * ((i - center) * 1.5f);
        }

        // ─── Circle ─────────────────────────────────────────────────
        private static Vector3 CircleOffset(int i, int total)
        {
            float angle = i * (360f / total) * Mathf.Deg2Rad;
            const float radius = 2f;
            return new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        }

        // ─── Grid (сетка) ───────────────────────────────────────────
        // Число колонок ограничено числом юнитов, неполный ряд центрируется,
        // ряды центрируются по Z. Для одного юнита оффсет = 0.
        public static Vector3 GridOffset(
            int i, int total, Vector3 dir,
            int unitsPerRow, float spacingX, float spacingZ)
        {
            Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;

            int cols = Mathf.Max(1, Mathf.Min(unitsPerRow, total));
            int totalRows = Mathf.CeilToInt((float)total / cols);
            int row = i / cols;
            int col = i % cols;
            int inRow = (row == totalRows - 1) ? total - row * cols : cols;

            float offsetX = (col - (inRow - 1) * 0.5f) * spacingX;
            float offsetZ = (row - (totalRows - 1) * 0.5f) * spacingZ;

            return right * offsetX - dir * offsetZ;
        }

        public struct GridParams
        {
            public int UnitsPerRow;
            public float SpacingX;
            public float SpacingZ;

            public static GridParams Default => new GridParams
            {
                UnitsPerRow = 3,
                SpacingX = 1.5f,
                SpacingZ = 1.5f
            };
        }
    }
}
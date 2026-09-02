using UnityEngine;

namespace Galactic1.Utility
{

    public static class MouseDirectionUtils
    {
        /// <summary>
        /// Проверяет, соответствует ли направление движения мыши заданному направлению, с учетом "фриза" второй оси.
        /// </summary>
        /// <param name="previousMousePosition">Позиция мыши в предыдущем кадре (в мировых координатах)</param>
        /// <param name="currentMousePosition">Текущая позиция мыши (в мировых координатах)</param>
        /// <param name="requiredDirection">Ожидаемое направление движения (например, Vector2.right)</param>
        /// <param name="angleToleranceDegrees">Допустимое отклонение в градусах (например, 45)</param>
        /// <returns>True, если движение мыши соответствует требуемому направлению</returns>
        public static bool IsMouseMovingInDirection(
            Vector2Int lastGridPosition,
            Vector2Int currentGridPos,
            Vector2 requiredDirection)
        {
            Vector2Int delta = currentGridPos - lastGridPosition;

            // Определяем ось, если ещё не зафиксирована
            // if (!axisLocked)
            // {
            //     if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            //         lockXAxis = true;
            //     else
            //         lockXAxis = false;
            //
            //     axisLocked = true;
            // }

            // Принудительно замораживаем вторую ось
            if (requiredDirection.x != 0)
                currentGridPos.y = lastGridPosition.y;
            else
                currentGridPos.x = lastGridPosition.x;

            if (currentGridPos == lastGridPosition)
                return false;

            // Vector2 worldCurrent = GridToWorld(currentGridPos);
            // Vector2 worldLast = GridToWorld(lastGridPosition);
            Vector2 dir = ((Vector2)currentGridPos - (Vector2)lastGridPosition).normalized;

            // Убираем вторую ось из сравнения
            if (Mathf.Abs(requiredDirection.x) > Mathf.Abs(requiredDirection.y))
                dir.y = 0;
            else
                dir.x = 0;

            dir.Normalize();

            float dot = Vector2.Dot(dir, requiredDirection.normalized);
            bool isForward = dot > Mathf.Cos(45f * Mathf.Deg2Rad);
            bool isBackward = dot < -Mathf.Cos(45f * Mathf.Deg2Rad);

            if (isForward)
            {
                lastGridPosition = currentGridPos;
                return true;
            }
            
            if (isBackward)
            {
                lastGridPosition = currentGridPos;
                return false;
            }

            return false;
        }



        /// <summary>
        /// Проверяет, достаточно ли клеток между стартовой позицией и мышкой,
        /// чтобы уместился объект нужного размера по выбранной оси.
        /// </summary>
        public static bool HasEnoughSpaceBetween(
            Vector2Int start,
            Vector2Int mouse,
            Vector2Int sizeInCells,
            bool horizontal
        )
        {
            int distance = horizontal
                ? Mathf.Abs(mouse.x - start.x)
                : Mathf.Abs(mouse.y - start.y);

            int requiredCells = horizontal ? sizeInCells.x : sizeInCells.y;

            return distance >= requiredCells;
        }
    }

}
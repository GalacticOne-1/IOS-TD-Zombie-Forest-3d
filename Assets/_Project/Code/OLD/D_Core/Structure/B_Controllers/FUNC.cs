using UnityEngine;

namespace Galactic1
{

    public static class FUNC
    {
        
        /// <summary>
        /// Случайная точка недалеко от границы внутри  
        /// </summary>
        /// <param name="squareCenter"></param>
        /// <param name="size"></param>
        /// <param name="maxDistanceFromBorder"></param>
        /// <returns></returns>
        public static Vector2 GetRandomPointInsideSquare2D(Vector2 squareCenter,  Vector2 size, float maxDistanceFromBorder = 10f)
        {
            // отступ от границы
            float _border = 2;
            
            // Clamp maxDistanceFromBorder so it doesn't exceed half of size
            float clampedDistanceX = Mathf.Min(maxDistanceFromBorder, size.x / 2f);
            float clampedDistanceY = Mathf.Min(maxDistanceFromBorder, size.y / 2f);

            // Decide randomly which border to pick (0 = left, 1 = right, 2 = top, 3 = bottom)
            int border = Random.Range(0, 4);

            Vector2 point = Vector2.zero;

            switch (border)
            {
                case 0: // Left border
                    point.x = Random.Range(_border, clampedDistanceX);
                    point.y = Random.Range(_border, size.y-_border);
                    break;
                case 1: // Right border
                    point.x = Random.Range(size.x - clampedDistanceX, size.x-_border);
                    point.y = Random.Range(_border, size.y-_border);
                    break;
                case 2: // Top border
                    point.x = Random.Range(_border, size.x-_border);
                    point.y = Random.Range(size.y - clampedDistanceY, size.y- (_border + 2));
                    break;
                case 3: // Bottom border
                    point.x = Random.Range(_border, size.x-_border);
                    point.y = Random.Range(_border, clampedDistanceY);
                    break;
            }

            return point;
        }
        
        /// <summary>
        /// Случайная точка снаружи
        /// </summary>
        /// <param name="squareCenter"></param>
        /// <param name="squareSize"></param>
        /// <param name="minDistanceOutside"></param>
        /// <returns></returns>
        public static Vector2 GetRandomPointOutsideSquare2D(Vector2 squareCenter, Vector2 squareSize, float minDistanceOutside = 1f)
        {
            Vector2 halfSize = squareSize / 2f;

            // Define boundaries
            float left = squareCenter.x - halfSize.x;
            float right = squareCenter.x + halfSize.x;
            float bottom = squareCenter.y - halfSize.y;
            float top = squareCenter.y + halfSize.y;

            Vector2 point;

            // Randomly choose which side to generate the point on: 0 = left, 1 = right, 2 = bottom, 3 = top
            int side = Random.Range(0, 4);

            switch (side)
            {
                case 0: // Left
                    point = new Vector2(Random.Range(left - minDistanceOutside - 10f, left - minDistanceOutside),
                        Random.Range(bottom - 10f, top + 10f));
                    break;
                case 1: // Right
                    point = new Vector2(Random.Range(right + minDistanceOutside, right + minDistanceOutside + 10f),
                        Random.Range(bottom - 10f, top + 10f));
                    break;
                case 2: // Bottom
                    point = new Vector2(Random.Range(left - 10f, right + 10f),
                        Random.Range(bottom - minDistanceOutside - 10f, bottom - minDistanceOutside));
                    break;
                default: // Top
                    point = new Vector2(Random.Range(left - 10f, right + 10f),
                        Random.Range(top + minDistanceOutside, top + minDistanceOutside + 10f));
                    break;
            }

            return point;
        }
        
    }
}
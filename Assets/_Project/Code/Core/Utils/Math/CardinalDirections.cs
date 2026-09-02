using UnityEngine;

namespace Galactic1.Utility
{
    public struct CardinalDirections
    {
        public readonly Vector2Int Up;
        public readonly Vector2Int Down;
        public readonly Vector2Int Left;
        public readonly Vector2Int Right;

        public CardinalDirections(bool init = true)
        {
            Up = Vector2Int.up; // (0, 1)
            Down = Vector2Int.down; // (0, -1)
            Left = Vector2Int.left; // (-1, 0)
            Right = Vector2Int.right; // (1, 0)
        }

        /// <summary>
        /// Получить направление по индексу: 0=Up, 1=Down, 2=Left, 3=Right
        /// </summary>
        public Vector2Int this[int index]
        {
            get
            {
                return index switch
                {
                    0 => Up,
                    1 => Down,
                    2 => Left,
                    3 => Right,
                    _ => Vector2Int.zero
                };
            }
        }

        /// <summary>
        /// Получить массив всех направлений
        /// </summary>
        public Vector2Int[] All => new[] { Up, Down, Left, Right };
    }

}
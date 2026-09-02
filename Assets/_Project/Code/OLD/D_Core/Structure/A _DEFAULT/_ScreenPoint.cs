using UnityEngine;

namespace Galactic1
{




    public class SCREEN_Raycast2D
    {
        /// <summary>
        /// Пускает луч от позиции курсора в сцену
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="hit"></param>
        /// <param name="startTouch"></param>
        public SCREEN_Raycast2D(LayerMask layer, out RaycastHit2D hit, out Vector2 startTouch)
        {
            startTouch = Input.mousePosition;
            hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(startTouch), Vector2.zero, 1 ,layer);
        }
    }
}
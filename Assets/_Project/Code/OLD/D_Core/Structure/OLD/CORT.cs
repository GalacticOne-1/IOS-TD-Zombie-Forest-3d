using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class CORT
    {

        /*
         *     Управление частями игры в техническом плане
         *     (отключение, переключение и пр) Камера, звук, сцены и тд
         *     Для каждого проекта
         */
        
        /// <summary>
        /// Блокировка всех кнопок (может игнорироватся самой кнопкой => ignoreBlock)
        /// </summary>
        public static bool BLOCK_BUTTTONS;

        public static GameObject blockScreen;//, loadPay;
        
        
        /// <summary>
        /// Центр экрана для UI (any resolution)
        /// </summary>
        /// <returns></returns>
        public static Vector2 CenterScreen() => new Vector2(Screen.width/2, Screen.height/2);


        #region Screen Scaler


        private static float GetScaleUI(int width, int height, Vector2 scalerReferenceResolution, float scalerMatchWidthOrHeight)
        {
            return Mathf.Pow(width/scalerReferenceResolution.x, 1f - scalerMatchWidthOrHeight)*
                   Mathf.Pow(height/scalerReferenceResolution.y, scalerMatchWidthOrHeight);
        }
        
        public static float GetScreenToWorldHeight
        {
            get
            {
                Vector2 topRightCorner = new Vector2(1, 1);
                Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);
                var height = edgeVector.y * 2;
                return height;
            }
        }
        public static float GetScreenToWorldWidth
        {
            get
            {
                Vector2 topRightCorner = new Vector2(1, 1);
                Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);
                var width = edgeVector.x * 2;
                return width;
            }
        }
        

        #endregion

        /// <summary>
        /// Общая блокировка экрана
        /// </summary>
        /// <param name="Y"></param>
        public static void BlockScreen(bool Y) => blockScreen.SetActive(Y);

        /// <summary>
        /// Блокировка экрана при покупке в магазине, пока грузится оплата
        /// </summary>
        /// <param name="y"></param>
        //public static void LoadPay(bool y) => loadPay.SetActive(y);

        
        /// <summary>
        /// Обычная скорость игры
        /// </summary>
        public static void SpeedGame_regular()
        {
            Time.timeScale = 1;
        }
        
        /// <summary>
        /// Вернуть скорость после понижения
        /// </summary>
        public static void SpeedGame_restore()
        {
            SpeedBattle.I.RestoreSpeed();
        }
        
        /// <summary>
        /// Самая медленная скорость игры
        /// </summary>
        public static void SpeedGame_low()
        {
            Time.timeScale = .1f;
        }

        
        /// <summary>
        /// Звук error
        /// </summary>
        public static void SoundAlert() => ServiceLocator.Current.Get<AudioController>().Sound_UI(3);
        
        /// <summary>
        /// Показать плашку (нe хватает кристаллов)
        /// </summary>
        public static void NotEnoughtGems()
        {
            
        }
        

        /*
         *    ----------------------------- ^ НЕ ТРОГАТЬ ^
         */










    }
}
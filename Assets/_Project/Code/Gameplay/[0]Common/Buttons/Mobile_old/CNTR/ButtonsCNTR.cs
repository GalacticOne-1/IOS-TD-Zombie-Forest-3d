using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    public class ButtonsCNTR
    {
        /*
         *      Управление двумя кнопками качелями -/+ 
         */

        
        #region FIELDS

        private GameObject bLeft, bRight;

        private Sprite[] sprites;
        // left_on =0, left_off =1, righr_on =2, right_off =3


        private int number_max;         // доступное кол-во элементов
        private int number;             // текуший номер

        private DFunc1 onUpdate;


        
        public ButtonsCNTR(
            int _number_max, 
            DFunc1 _onUpdate, 
            GameObject _bLeft, 
            GameObject _bRight, 
            Sprite[] _sprites)
        {
            number_max = _number_max-1;
            onUpdate = _onUpdate;
            bLeft = _bLeft;
            bRight = _bRight;
            sprites = _sprites;
            
            bLeft.EventBtnOne_old(Left);
            bRight.EventBtnOne_old(Right);
        }
        
        

        #endregion




        /// <summary>
        /// Для сброса к первому элементу или id с которго надо стартовать
        /// <br/>(Вызывется после конструктора)
        /// </summary>
        /// <param name="number_start">id element</param>
        public void ResetState(int number_start = 0)
        {
            number = number_start;
            onUpdate?.Invoke(number);
            StateButtons();
        }

        
        /// <summary>
        /// Кнопка -
        /// </summary>
        public void Left()
        {
            if (number > 0)
            {
                number--;
                onUpdate?.Invoke(number);
            }
            StateButtons();
        }

        
        /// <summary>
        /// Кнопка +
        /// </summary>
        public void Right()
        {
            if (number < number_max)
            {
                number++;
                onUpdate?.Invoke(number);
            }
            StateButtons();
        }

        void StateButtons()
        {
            bLeft.GetComponent<Image>().sprite = number > 0 ? sprites[0] : sprites[1];
            bRight.GetComponent<Image>().sprite = number < number_max ? sprites[2] : sprites[3];
        }
    }
}
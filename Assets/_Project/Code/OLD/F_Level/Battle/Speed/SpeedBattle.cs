using Galactic1;
using TMPro;
using UnityEngine;

namespace Galactic1
{
    public class SpeedBattle : Singleton<SpeedBattle>
    {
        [SerializeField] private TextMeshProUGUI tSpeed;


        private float[] speed =
        {
            1,
            1.5f,
            2f,
            2.5f,
            10
        };

        private string[] speedTitle =
        {
            "1", "2", "3", "4", "10"
        };




        
        /// <summary>
        /// Сброс на x1
        /// </summary>
        public void ResetSpeed()
        {
            // GAMEPLAY_old.DataGameplay().speedBattle = 0;
            // Time.timeScale = speed[GAMEPLAY_old.DataGameplay().speedBattle];
            // tSpeed.text = $"x{speedTitle[GAMEPLAY_old.DataGameplay().speedBattle]}";
        }

        
        // настройка скорости в бою
        public void SetSpeed()
        {
            // GAMEPLAY_old.DataGameplay().speedBattle++;
            // if (GAMEPLAY_old.DataGameplay().speedBattle >= (ApplicationSetup.I.MODE_REGULAR ? speed.Length-1 : speed.Length))
            //     GAMEPLAY_old.DataGameplay().speedBattle = 0;
            //
            // Time.timeScale = speed[GAMEPLAY_old.DataGameplay().speedBattle];
            // tSpeed.text = $"x{speedTitle[GAMEPLAY_old.DataGameplay().speedBattle]}";
        }

        // восстанавливаем скорость для боя
        public void RestoreSpeed()
        {
            // Time.timeScale = speed[GAMEPLAY_old.DataGameplay().speedBattle];
            // tSpeed.text = $"x{speedTitle[GAMEPLAY_old.DataGameplay().speedBattle]}";
        }
    }
}
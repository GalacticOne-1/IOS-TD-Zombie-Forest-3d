using UnityEngine;
using Galactic1.Core;

namespace Galactic1
{
    public class AppService : MonoBehaviour
    {
        private void OnApplicationFocus(bool hasFocus)
        {
            if (_GameState.AppLoaded)
            {
                if (hasFocus)
                {
                    // сброс блокировки, чтобы не блокировать игру
                    //CORT.LoadPay(false);

                    // if (USE_SERVER)
                    // {
                    //     new SERVER_Connect();
                    // }
                }
                // игрок свернул игру
                else
                {
                    TimeManagement.SaveCurrTime();
                    //GAMEPLAY_old.Saving();
                }
            }
        }

        private void OnApplicationQuit()
        {
            TimeManagement.SaveCurrTime();
            //GAMEPLAY_old.Saving();
        }
    }
}
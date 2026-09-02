using Galactic1.Repository;
using Galactic1;
using UnityEngine;

namespace Galactic1
{





    public class LOCATION
    {
        private int id;

        public LOCATION(int id)
        {
            this.id = id;
        }

        
        /// <summary>
        /// Для перехода в локацию
        /// </summary>
        public void Load()
        {
            // ServiceLocator.Current.Get<GlobalRepository>().CurrLocation = id;
            // ServiceLocator.Current.Get<GameMachine>().MODE = GameMachine.EMode.REGULAR;
            // ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.LEVEL_PLAY);
            // DLog.Alert($"Start mission {ServiceLocator.Current.Get<GlobalRepository>().CurrLocation}");
        }

        public void Unload()
        {
            
        }
    }
    
    
    public class LOCATION_SETUP
    {
        /// <summary>
        /// Коллайдеры для выхода с локации
        /// </summary>
        /// <param name="y"></param>
        public void LocationExit(bool y)
        {
            //var obj = GameObject.FindObjectsOfType<LocationExit>(true);
            //obj[0].gameObject.SetActive(y);
            //obj[1].gameObject.SetActive(y);
        }


        public void SetGroundBorderX(Vector2 v) {}//JoystickController.I.borderX = v;
        public void SetGroundBorderY(Vector2 v) {} //JoystickController.I.borderY = v;
    }
}
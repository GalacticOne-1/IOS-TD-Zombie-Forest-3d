

namespace Galactic1
{

    /*
     *      Управление режимaми в игре
     *      Меняет поведение и экраны
     */



    public class GameMode_Regular
    {
        /// <summary>
        /// Обычное поведение игрока (движение, взаимодействие, атака и тд)
        /// </summary>
        public GameMode_Regular()
        {
            // #1 убираем канвас
            ServiceLocator.Current.Get<ViewGameController>().SetButtons(true);
            //ServiceLocator.Current.Get<CampBattle>().CTime.SetActive(false);
            
            // #2 меняем камеры
            CameraFollow.I.STOP = false;
            //CameraFollow.I.SetPosition(HUBLink.player.tr.position);
            CameraControllerOld.I.STOP = true;
            
            // #3 выключаем сетку
            //Construction.I.Clear();
            //ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(Construction.I);
            //GAMEPLAY_old.CONSTRUCTION_MODE = EConstructMode.Free;
            //HUBGrid.I.gridHold.gameObject.SetActive(false);
        }
    }
    
    
    public class GameMode_Construct
    {
        /// <summary>
        /// Для строительства
        /// </summary>
        public GameMode_Construct()
        {
            // #1 убираем канвас
            ServiceLocator.Current.Get<ViewGameController>().SetButtons(false);
            //ServiceLocator.Current.Get<CampBattle>().CTime.SetActive(true);
            
            // #2 меняем камеры
            CameraFollow.I.STOP = true;
            CameraControllerOld.I.STOP = false;

            // #3 включаем сетку
            // Construction.I.requireFloor = -1;
            // Construction.I.SetColor();
            // ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(Construction.I);
            // GAMEPLAY_old.CONSTRUCTION_MODE = EConstructMode.Moving;
            // HUBGrid.I.gridHold.gameObject.SetActive(true);
        }
    }
}
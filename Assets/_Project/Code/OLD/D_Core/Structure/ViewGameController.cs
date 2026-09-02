using Galactic1;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1
{
    
    public class ViewGameController : MonoBehaviour, IGameService
    {
        /*
         *      Установщик окон и управление 
         */

        [SerializeField] private GameObject[] canvas;

        [SerializeField] private GameObject[] menu;
        [SerializeField] private GameObject[] buttons;

        [SerializeField] // объекты статы
        private GameObject[] containerStat;

        [SerializeField] private GameObject[] widgets;

        [SerializeField] private CTaskLogger taskLogger;
        [System.Serializable] 
        public struct CTaskLogger
        {
            public GameObject holdGame, holdMap;
            public GameObject panel;
        }
        
        
        


        
        
        // new level
        [SerializeField] private NewLevelView newLevelView;
        private NewLevelViewModel newLevelViewModel;
        public NewLevelViewModel NewLevelViewModel => newLevelViewModel;




        // окно окончания игры
        [SerializeField] private FinishLevelView finishLevelView;
        private FinishLevelViewModel finishLevelPresenter;
        public FinishLevelViewModel FinishLevelPresenter => finishLevelPresenter;


        [Header("MAIN MENU SCREEN")]
        // *** main menu
        [SerializeField] private MainMenuView mainMenuView;
        private MainMenuViewModel mainMenuViewModel;
        public MainMenuViewModel MainMenuViewModel => mainMenuViewModel;


        // -------------        ^^  DEFAULT  ^^
        
        

        // бокс снаряжения за рекламу
        // [SerializeField] private EquipmentADBoxView equipmentADBoxView;
        // private EquipmentADBoxViewModel equipmentADBoxViewModel;
        // public EquipmentADBoxViewModel EquipmentADBoxViewModel => equipmentADBoxViewModel;
        //
        // // торговец бонусами в лагере игрока
        // [SerializeField] private CampBonusView campBonusView;
        // private CampBonusViewModel campBonusViewModel;
        // public CampBonusViewModel CampBonusViewModel => campBonusViewModel;


        [SerializeField] private DetailObjView detailObjView;
        private DetailObjViewModel detailObjViewModel;
        public DetailObjViewModel DetailObjViewModel => detailObjViewModel;





        // ----------------------------------
        
        
        
        
        
        public void Init()
        {
            
            // ***      NEW LEVEL
            var newLevelModel = new NewLevelModel(newLevelView);
            newLevelViewModel = new NewLevelViewModel(newLevelModel, newLevelView);
            newLevelView.Init(newLevelViewModel);
            
            
            
            
            // ***      START SCREEN
            
            
            // finish screen
            var finishLevelModel = new FinishLevelModel(finishLevelView);
            finishLevelPresenter = new FinishLevelViewModel(finishLevelModel, finishLevelView);
            finishLevelView.Init(finishLevelPresenter);

            // ***      EXIT SCREEN
            
            
            
            
            // ***   CENTER SCREEN    ***
            
            
            //  -- daily quest
            //var questModel = new QuestModel(questView);
            //questViewModel = new QuestViewModel(questModel, questView);
            //questView.Init(questViewModel);
            
            
            // *** MAIN MENU
            // меню должно загружаться первым т.к от его флагов зависят другие системы
            var mainMenuModel = new MainMenuModel(mainMenuView);
            mainMenuViewModel = new MainMenuViewModel(mainMenuModel, mainMenuView);
            mainMenuView.Init(mainMenuViewModel);
            
            
            // --------------------------------- ^^^ DEFAULT ^^^ -------------------------------------------

            
            // бокс снаряжения за рекламу
            // var equipmentADBoxModel = new EquipmentADBoxModel(equipmentADBoxView);
            // equipmentADBoxViewModel = new EquipmentADBoxViewModel(equipmentADBoxModel, equipmentADBoxView);
            // equipmentADBoxView.Init(equipmentADBoxViewModel);
            //
            // // торговец бонусами
            // var campBonusModel = new CampBonusModel(campBonusView);
            // campBonusViewModel = new CampBonusViewModel(campBonusModel, campBonusView);
            // campBonusView.Init(campBonusViewModel);
            
            
            
            // #..      основной виджет
            // -- ..    мелкие виджеты для основного (могут быть вложены) 
            
            
            // ***        MAIN MENU SCREEN        ***
            
            // * детальная инфо по объекту
            var detailObjModel = new DetailObjModel(detailObjView);
            detailObjViewModel = new DetailObjViewModel(detailObjModel, detailObjView);
            detailObjView.Init(detailObjViewModel);
            
            
            

        }




        
        
        /// <summary>
        /// Управление канвасами
        /// </summary>
        /// <param name="required"></param>
        public void GetCanvas(ECanvas[] required)
        {
            var l = canvas.Length;
            for (int i = 1; i < l; i++)
                canvas[i].SetActive(false);

            // открываем нужные
            l = required.Length;
            for (int i = 0; i < l; i++)
                canvas[(int)required[i]].SetActive(true);
        }
        
        /// <summary>
        /// Вернет объект канваса
        /// </summary>
        /// <param name="required"></param>
        /// <returns></returns>
        public GameObject GetCanvas(ECanvas required) => canvas[(int)required];
        
        
        /// <summary>
        /// Управление отображением меню
        /// </summary>
        /// <param name="required"></param>
        public void GetMenu(EMenu[] required)
        {
            var l = menu.Length;
            for (int i = 1; i < l; i++)
                menu[i].SetActive(false);

            // открываем нужные
            l = required.Length;
            for (int i = 0; i < l; i++)
                menu[(int)required[i]].SetActive(true);
        }
        
        /// <summary>
        /// Вернет объект меню
        /// </summary>
        /// <param name="required"></param>
        /// <returns></returns>
        public GameObject GetMenu(EMenu required) => menu[(int)required];

        
        /// <summary>
        /// Управление видимиостью основных кнопок (игрок, меню)
        /// </summary>
        /// <param name="show"></param>
        public void SetButtons(bool show)
        {
            var l = buttons.Length;
            for (int i = 0; i < l; i++)
                buttons[i].SetActive(show);
        }

        /// <summary>
        /// Перемещение между канвасами панели заданий
        /// </summary>
        /// <param name="map"></param>
        public void SetTaskLogger(bool map)
        {
            taskLogger.panel.transform.parent = map ? taskLogger.holdMap.transform : taskLogger.holdGame.transform;
        }
        
        
        /// <summary>
        /// Управление отображением элементов статистики
        /// </summary>
        /// <param name="required"></param>
        public void GetStats(EBankResourceType[] required)
        {
            containerStat.AllElementsOff();
            var l = required.Length;
            for (int i = 0; i < l; i++)
            {
                containerStat[(byte)required[i]].SetActive(true);
            }
        }

        
        /// <summary>
        /// Для закрытия всех активных окон
        /// </summary>
        public void CloseActiveScreens()
        {
            var l = widgets.Length;
            for (int i = 0; i < l; i++)
            {
                if (widgets[i] && widgets[i].activeSelf)
                {
                    if(widgets[i].GetComponent<IWidgetClose>() != null)
                        widgets[i].GetComponent<IWidgetClose>().RequireClosing();
                }
            }
        }
        
        

        
        

        /// <summary>
        /// Вызов нужного виджета
        /// </summary>
        /// <param name="required"></param>
        public void GetWindow(EWindow required)
        {
            switch (required)
            {
                case EWindow.START:
                    
                    break;
                case EWindow.PAUSE:
                    
                    break;
                case EWindow.FINISH:
                    
                    break;
                
            }
        }
        
        /// <summary>
        /// Вызов спец. виджета с настройкой
        /// </summary>
        public void GetWindow<T>(EWindow required, T data)
        {
            switch (required)
            {
                case EWindow.FINISH:
                {
                    finishLevelPresenter.OpenWindow(data);
                } break;
                
                
                case EWindow.LEVEL_UP_PLAYER:
                {
                    newLevelViewModel.OpenWindow(data);
                } break;
                
                case EWindow.NEW_UNIT:
                {
                    
                } break;
                
                case EWindow.LEVEL_UP_UNIT:
                {
                    
                } break;
                
                case EWindow.NEW_MODIFICATOR:
                {
                    
                } break;
                
                case EWindow.NEW_LOCATION:
                {
                    
                } break;

                case EWindow.EQUIPMENT_UPGRADE:
                {
                    
                } break;
            }
        }


        
    }


    public interface IWidgetClose
    {
        void RequireClosing();
    }
}
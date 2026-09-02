using Galactic1;
using UnityEngine;

namespace Galactic1
{
    /*    Единичный класс CORE
     *    Ууправление камерой для Gameplay
     *    Имеет проверку на движение экрана, необходимую для некоторых кликов
     */
    public class CameraControllerOld : Singleton<CameraControllerOld>, IUpdate
    {
        [Header("Игровая камера")] 
        public GameObject cameraHold;
        Camera gameCamera;
        public Camera GameCamera => gameCamera;

        [SerializeField] private float smooth = 1, autoMoveSmooth = 1;
        [SerializeField] private Vector2 borderMin, borderMax;

        [SerializeField] private Vector3 startPos;
        private Vector3 cashCameraPos, cashCursorPos;
        
        
        private Vector3 curCameraPos, touchStart, diff;
        private Transform tr;
        public bool STOP;

        // auto movement
        [SerializeField] private GameObject autoMovePanel;
        private bool autoMove;
        private Vector3 endAutoMove;

        private bool ACTIVATED;




        #region Init


        public void Activator()
        {
            // камера должна быть вложена, нужно для shaker
            if (cameraHold && cameraHold.transform.childCount > 0)
            {
                gameCamera = cameraHold.GetChild(0).GetComponent<Camera>();
            }
            
            if (gameCamera == null)
            {
                Debug.LogError("Для корректной работы требуется Game Camera. <CameraManagement>");
                return;
            }

            ACTIVATED = true;
            tr = cameraHold.transform;
            tr.position = startPos;
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
            //GameSetup.I.onResetUpdate += IUpdateClear;
        }
        
        
        
        public void IUpdateClear()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }
        
        
        
        /// <summary>
        /// Запуск камеры для авто движения в начале игры
        /// </summary>
        public void NewGame()
        {
            if (DeveloperConsole.I.core.camera_dev)
            {
                autoMovePanel.SetActive(false);
                ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
                //LevelSetup.I.onResetUpdate += IUpdateClear;
                return;
            }
            
            autoMovePanel.SetActive(true);
            autoMove = true;
            endAutoMove = borderMin;
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
            //LevelSetup.I.onResetUpdate += IUpdateClear;
        }
        
        /// <summary>
        /// Для отключения камеры
        /// </summary>
        public void Clear()
        {
            autoMove = false;
            tr.position = startPos;
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
            //LevelSetup.I.onResetUpdate -= IUpdateClear;
        }

        #endregion



        #region Action


        public void ResetPosition()
        {
            tr.position = startPos;
        }


        public void UpdateM()
        {
            // if (STOP) return;
            //
            // if (autoMove)
            // {
            //     tr.position = Vector3.MoveTowards(tr.position, endAutoMove, autoMoveSmooth * Time.deltaTime);
            //     if (tr.position == endAutoMove)
            //     {
            //         autoMovePanel.SetActive(false);
            //         autoMove = false;
            //     }
            //     
            //     return;
            // }
            //
            // // ------------ двигаем камеру перетаскиванием
            // // одинаково работает на пк и мобилах
            // if (!UIController.I.UI_ELEMENT && GAMEPLAY_old.CONSTRUCTION_DRAG == EConstructionDrag.NON)
            // {
            //     if (Input.GetMouseButtonDown(0))
            //     {
            //         touchStart = CursorManagement.I.CursorCoord();
            //     }
            //
            //     if (Input.GetMouseButton(0))
            //     {
            //         diff = touchStart - CursorManagement.I.CursorCoord();
            //         curCameraPos = tr.position;
            //         curCameraPos += diff;
            //         curCameraPos *= smooth;
            //         curCameraPos.x = Mathf.Clamp(curCameraPos.x, borderMin.x, borderMax.x);
            //         curCameraPos.y = Mathf.Clamp(curCameraPos.y, borderMin.y, borderMax.y);
            //         tr.position = curCameraPos;
            //     }
            // }
           
            
            // старый вариант, для пк работает без проблем, для мобил НЕТ
            /*if (!UIController.I.UI_ELEMENT && GAMEPLAY.CONSTRUCTION_DRAG == EConstructionDrag.NON && Input.GetMouseButton(0))
            {
                touchStart = CursorManagement.I.CursorCoord();
                diff = lastCameraPos - touchStart;
                diff *= smooth;
            
                diff.x = Mathf.Clamp(diff.x + tr.position.x, borderMin.x, borderMax.x);
                diff.x -= tr.position.x;
                diff.y = Mathf.Clamp(diff.y + tr.position.y, borderMin.y, borderMax.y);
                diff.y -= tr.position.y;
                tr.Translate(diff); 
            }
            lastCameraPos = CursorManagement.I.CursorCoord();*/
            
        }

        

        // для внешного запуска движения
        public void MoveCamera()
        {
            
            // ------------ двигаем камеру перетаскиванием
            if (!UIController.I.UI_ELEMENT)
            {
                touchStart = CursorManagement.I.CursorCoord();
                diff = curCameraPos - touchStart;

                diff.x = Mathf.Clamp(diff.x + tr.position.x, borderMin.x, borderMax.x);
                diff.x -= tr.position.x;
                diff.y = Mathf.Clamp(diff.y + tr.position.y, borderMin.y, borderMax.y);
                diff.y -= tr.position.y;
                tr.Translate(diff); 
            }
        }


        /// <summary>
        /// Позиция для hold камеры
        /// </summary>
        /// <param name="coord"></param>
        public void SetPosition(Vector2 coord)
        {
            coord.x = Mathf.Clamp(coord.x, borderMin.x, borderMax.x);
            coord.y = Mathf.Clamp(coord.y, borderMin.y, borderMax.y);
            tr.position = new Vector3(coord.x, coord.y, -10);
            curCameraPos = tr.position;
        }

        

        #endregion
        
        
        
        #region Bool - Change Position Screen
        /// <summary>
        /// Берет позицию экрана, для последующего сравнения
        /// </summary>
        public void CashScreenPos()
        {
            if (ACTIVATED)
            {
                cashCameraPos = gameCamera.transform.position;
                cashCursorPos = gameCamera.GetMouseWorldPosZ();
            }
        }

        /// <summary>
        /// true - экран не двигался 
        /// </summary>
        /// <returns></returns>
        public bool ScreenFrozen()
        {
            var coord = cashCameraPos - gameCamera.transform.position;
            var coord2 = cashCursorPos - gameCamera.GetMouseWorldPosZ();

            return Mathf.Abs(coord.x) < .3f && Mathf.Abs(coord.y) < .3f
                && Mathf.Abs(coord2.x) < .3f && Mathf.Abs(coord2.y) < .3f;
        }

        public bool CursorFrozen()
        {
            var coord = cashCursorPos - gameCamera.GetMouseWorldPosZ();
            return Mathf.Abs(coord.x) < .3f && Mathf.Abs(coord.y) < .3f;
        }

        #endregion


        
    }
}
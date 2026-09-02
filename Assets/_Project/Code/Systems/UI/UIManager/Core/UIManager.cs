using System;
using System.Collections;
using Galactic1.Code.UI.Core;
using Galactic1.UI.Core.TabPanel;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.UI.Core
{
    [DefaultExecutionOrder(-1000)]
    public class UIManager : MonoBehaviour, IGameService
    {
        private DIContainer _container;
        
        [System.Serializable] 
        public struct CTransformRoot
        {
            public Transform screensRoot;
            public Transform overlayRoot;
            public Transform hudRoot;
            public Transform hudWorldRoot;    // элементы геймплея живущие всю сцену
            public Transform constructionRoot;
            public Transform floatWorldRoot;  // всплывающие элементы геймплея в сценe
            public Transform popupRoot;
        }
       
        [field: SerializeField] public CTransformRoot TransformRoot { get; private set; }

        
        private UIPrefabProvider prefabProvider;
        private UITransitionService transitionService;
        private UIScreenManager screenManager;
        private UIPopupManager popupManager;

        private UIStackNavigator navigator;

        
        
        public UIScreenManager ScreenManager => screenManager;
        public UIPopupManager PopupManager => popupManager;

        public event Action<UIScreenId> OnScreenChanged;

        
        
        public void Initialize(DIContainer container)
        {
            _container = container;
            
            // Проверка ссылок
            if (prefabProvider == null) prefabProvider = GetComponentInChildren<UIPrefabProvider>();
            if (transitionService == null) transitionService = GetComponentInChildren<UITransitionService>();
            if (screenManager == null) screenManager = GetComponentInChildren<UIScreenManager>();
            if (popupManager == null) popupManager = GetComponentInChildren<UIPopupManager>();

            // Установка родителей
            prefabProvider.transform.SetParent(transform);
            transitionService.transform.SetParent(transform);
            screenManager.transform.SetParent(transform);
            popupManager.transform.SetParent(transform);

            // Инициализация менеджеров
            screenManager.Initialize(_container, prefabProvider, transitionService, TransformRoot);
            popupManager.Initialize(_container, prefabProvider, transitionService);

            navigator = new UIStackNavigator();


            // *** root всегда должны быть активны
            EventBus<SceneReadyEvent>.Register(new EventBinding<SceneReadyEvent>(() =>
            {
                TransformRoot.screensRoot.gameObject.SetActive(true);
                TransformRoot.overlayRoot.gameObject.SetActive(true);
                TransformRoot.hudRoot.gameObject.SetActive(true);
                TransformRoot.constructionRoot.gameObject.SetActive(true);
                TransformRoot.popupRoot.gameObject.SetActive(true);
                
                
                // *** активируем поля в сцене
                var allWidgets = 
                    FindObjectsByType<ReactiveWidgetBase>(
                        FindObjectsInactive.Include, 
                        FindObjectsSortMode.None);
                
                var l = allWidgets.Length;
                for (int i = 0; i < l; i++)
                    allWidgets[i].Initialize();
                // ***
                
                
                // *** что бы все кнопки получали конфиг
                var allButtons = 
                    FindObjectsByType<BaseUIButton>(
                        FindObjectsInactive.Include, 
                        FindObjectsSortMode.None);

                l = allButtons.Length;
                for (int i = 0; i < l; i++)
                    allButtons[i].Initialize(container);
                // ***
                
                DLog.Alert("Scene ready!");
            }));
            

            Debug.Log("[UIManager] Initialized AAA UI Framework");
        }

        private void CreateIfNull(ref Transform t, string name)
        {
            if (t != null) return;
            var go = new GameObject($"UI_{name}");
            go.transform.SetParent(transform);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            t = go.transform;
        }

        #region Screens

        public void OpenScreen(
            UIScreenId id, 
            object data = null, 
            Action<GameObject> onShow = null, 
            bool addToHistory = true)
        {

#if UNITY_EDITOR
            DLog.Alert($"Open screen {id}");            
#endif
            
            if (!IsManagementScreen(id, data, onShow, addToHistory))
                StartCoroutine(OpenScreenRoutine(id, data, onShow, addToHistory));
        }

        private IEnumerator OpenScreenRoutine(UIScreenId id, object data, Action<GameObject> onShow, bool addToHistory)
        {
            yield return StartCoroutine(screenManager.OpenScreen(id, data, onShow));
            if (addToHistory) navigator.Push(id);
            OnScreenChanged?.Invoke(id);
        }

        public void GoBack()
        {
            var prev = navigator.Pop();
            if (prev != null) OpenScreen(prev, null, null, false);
        }

        #endregion

        #region Popups

        public void OpenPopup(UIScreenId id, object data = null) => popupManager.OpenPopup(id, data);
        public void ClosePopup(UIScreenId id) => popupManager.ClosePopup(id);

        #endregion


        #region Layers

        public void EnterConstructionMode()
        {
            TransformRoot.hudWorldRoot.gameObject.SetActive(false);
            TransformRoot.hudRoot.gameObject.SetActive(false);
            TransformRoot.overlayRoot.gameObject.SetActive(false);

            TransformRoot.constructionRoot.gameObject.SetActive(true);
        }

        public void ExitConstructionMode()
        {
            TransformRoot.hudWorldRoot.gameObject.SetActive(true);
            TransformRoot.hudRoot.gameObject.SetActive(true);
            TransformRoot.overlayRoot.gameObject.SetActive(true);

            TransformRoot.constructionRoot.gameObject.SetActive(false);
        }

        #endregion


        #region Management Menu

        // перехватывает открытие паенли если она часть вкладок
        bool IsManagementScreen(
            UIScreenId id, 
            object data = null, 
            Action<GameObject> onShow = null, 
            bool addToHistory = true)
        {
            if (!IsTab(id)) 
                return false;

            ServiceLocator.Current.Get<TabPanelController>().SwitchTo(id, onShow, data);

            return true;
        }


        bool IsTab(UIScreenId id)
            => id switch
            {
                UIScreenId.FacilityList or
                    UIScreenId.FacilityPanel or
                    UIScreenId.Inventory or
                    UIScreenId.GameStore
                    => true,
                
                _ => false
            };

        #endregion
    }
}


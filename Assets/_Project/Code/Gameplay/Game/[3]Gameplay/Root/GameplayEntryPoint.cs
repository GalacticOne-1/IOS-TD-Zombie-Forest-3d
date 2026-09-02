
using Galactic1.Code.UI.Tooltips;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1
{
    
    /*   ! создается до ServiceLocator и конфиг провайдера !   */
    
    
    public class GameplayEntryPoint : MonoBehaviour
    {
        [SerializeField] private UIGameplayRootBinder _sceneUIRootPrefab;

        public void Run(DIContainer container)
        {
            DLog.Alert("======= Core scene loaded =======", AppConstants.show_log_core);
            
            // ***************************************************************************************************
            // ***************************************************************************************************
            
            // #1 регистрация сервисов
            var coreViewModelsContainer = new DIContainer(container);

            
            
#if UNITY_EDITOR
            // для читов
            //ServiceLocator.Current.Register(new CheatsService());
            //coreViewModelsContainer.RegisterFactory(_ => new CheatsService()).AsSingle();
#endif
            
            GameplayViewModelsRegistrations.Register(coreViewModelsContainer);
            
            // ***************************************************************************************************
            
            
            // #2 создали UI для сцены
            // это Root для вспего UI который динамически создается
            var uiRootView = coreViewModelsContainer.Resolve<UIRootView>();
            var uiSceneRootBinder = Instantiate(_sceneUIRootPrefab);
            uiRootView.AttachSceneUI(uiSceneRootBinder.gameObject);
            var rt = uiSceneRootBinder.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
            
            // заправшиваем рутовую вью модель и передаем в созданный байндер
            var uiSceneRootViewModel = coreViewModelsContainer.Resolve<UIGameplayRootViewModel>();
            uiSceneRootBinder.Bind(uiSceneRootViewModel);

            // отвечает за динамическое UI
            var uiManager = uiSceneRootBinder.GetComponent<UIManager>();
            container.RegisterInstance(uiManager);
            uiManager.Initialize(container);
            
            // === сервис подсказок во всей игре
            container.RegisterInstance(uiSceneRootBinder.GetComponent<TooltipController>());


            // ***************************************************************************************************
        }
    }
}
using Galactic1.Code.Core.State;
using Galactic1.Configs;
using UnityEngine;
using Galactic1.Core;
using Galactic1.UI.Core;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Runtime;

#if UNITY_IOS
using UnityEngine.iOS;
#endif


namespace Galactic1
{
    public class RWPanel : UIScreenPanel, IGameService
    {
        /*
         *    Панель оценки игры
         */
        
        [Tooltip("Отзыв запрашивается только после завершения этого шага обучения. Пусто = без ограничения.")]
        [SerializeField] private TutorialStepId requiredTutorialStep;

        [SerializeField] private GameObject bNo, BYes;




        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            
            ServiceLocator.Current.Register(this);

            bNo.RegisterButtonClick(OnHide);
            BYes.RegisterButtonClick(LoadAppPage);
        }
        
        public override void Remove()
        {
            base.Remove();

            EventBus<SceneServicesClearEvent>.Register(new EventBinding<SceneServicesClearEvent>(
                () => ServiceLocator.Current.Unregister<RWPanel>()));
        }


        public bool NeedRequest()
        {
            bool alreadyRequested =
                ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Rw.Value.rwRequest;

            if (alreadyRequested)
                return false;

            return IsTutorialGateOpen();
        }
        
        private bool IsTutorialGateOpen()
        {
            if (requiredTutorialStep == null)
                return true;

            // Сервиса обучения нет — гейтить нечем, не блокируем отзыв.
            if (!ServiceLocator.Current.TryGet<ITutorialService>(out var tutorial))
                return true;

            return tutorial.IsStepCompleted(requiredTutorialStep);
        }

        
        public override void OnShow(object data = null)
        {
            base.OnShow(data);

            //if (!ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>().requiresReviewService) 
                //return; // !TimeManagement.PassedDaysFromFirstLaunch(2)
            
            
            
            //if (!ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Review.Value.reviewRequest)
            //{
                StateWriter.Write(
                    ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Rw,
                    (ref CGameStateRW p) => { p.rwRequest = true; });
                
                // var proxy = ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Review.Value;
                // proxy.reviewRequest = true;
                // ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Review.Value =
                //     ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Review.Value;
                
                ServiceLocator.Current.Get<ScreenGFX>().PanelAnim1(gameObject, true);
            //}
        }

        public override void OnHide()
        {
            base.OnHide();
            ServiceLocator.Current.Get<ScreenGFX>().PanelAnim1(gameObject, false);
        }
        
        

        // открываем страницу магазина
        public void LoadAppPage()
        {
            OnHide();
            //ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Review.Value.review = true;
            ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Rw.Value =
                ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Rw.Value;
            

            if (ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>().isAppstore)
            {
#if UNITY_IOS
                Device.RequestStoreReview();
#endif
            }
            else
            {
                Application.OpenURL(AppConstants.GAME_PAGE);
            }
            
            DLog.Alert("Request rw!");
        }

    }
}
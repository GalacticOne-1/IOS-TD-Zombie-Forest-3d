using Galactic1.Code.Core.State;
using Galactic1.Configs;
using UnityEngine;
using Galactic1.Core;
using Galactic1.UI.Core;

#if UNITY_IOS
using UnityEngine.iOS;
#endif


namespace Galactic1
{
    public class Review : UIScreenPanel, IGameService
    {
        /*
         *    Панель оценки игры
         */

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
                () => ServiceLocator.Current.Unregister<Review>()));
        }


        public bool NeedRequest()
        {
            var d = ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>().requiresReviewService;
            var y = ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Review.Value.reviewRequest;
            return d && !y;
        }

        
        public override void OnShow(object data = null)
        {
            base.OnShow(data);

            //if (!ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>().requiresReviewService) 
                //return; // !TimeManagement.PassedDaysFromFirstLaunch(2)
            
            
            
            //if (!ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Review.Value.reviewRequest)
            //{
                StateWriter.Write(
                    ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Review,
                    (ref CGameStateReview p) => { p.reviewRequest = true; });
                
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
            ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Review.Value =
                ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Review.Value;
            

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
            
            DLog.Alert("Request review!");
        }

    }
}
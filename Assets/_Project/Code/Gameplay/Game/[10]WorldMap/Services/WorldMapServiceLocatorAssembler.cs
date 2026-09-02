using Galactic1.Code.Cameras;
using Galactic1.Code.Notification;
using Galactic1.Code.UI.RaidReport;
using Galactic1.Code.WorldMap;
using Galactic1.Configs;
using Galactic1.Core.Systems.GameSession;
using Galactic1.Game.UI.Stats;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1
{
    public class WorldMapServiceLocatorAssembler : MonoBehaviour
    {

        [SerializeField] private CameraControllerMap _cameraController;
        [SerializeField] private WorldMapController _worldMapController;
        [SerializeField] private RaidReportFlowController _raidReportFlowController;
        

        public void Initialize(DIContainer container)
        {
            var configProvider = container.Resolve<IConfigProvider>();
            
            ServiceLocator.Current.Register(new StatViewFactory(configProvider.Get<StatLayoutConfig>()));
            
            var notificationService = new NotificationService(configProvider.Get<NotificationMessageConfig>());
            ServiceLocator.Current.Register<INotificationService>(notificationService);
            
            ServiceLocator.Current.Register(FindAnyObjectByType<SceneSessionManager>());
            ServiceLocator.Current.Register(_cameraController as IMainCamera);
            ServiceLocator.Current.Register(_worldMapController);
            ServiceLocator.Current.Register(_raidReportFlowController);
            
            
            
            // ==== unregister
            var sceneServicesClear = new EventBinding<SceneServicesClearEvent>(() =>
            {
                ServiceLocator.Current.Unregister<StatViewFactory>();
                ServiceLocator.Current.Unregister<INotificationService>();
                ServiceLocator.Current.Unregister<SceneSessionManager>();
                ServiceLocator.Current.Unregister<IMainCamera>();
                ServiceLocator.Current.Unregister<WorldMapController>();
                ServiceLocator.Current.Unregister<RaidReportFlowController>();
                
            });
            EventBus<SceneServicesClearEvent>.Register(sceneServicesClear);
        }
    }
}
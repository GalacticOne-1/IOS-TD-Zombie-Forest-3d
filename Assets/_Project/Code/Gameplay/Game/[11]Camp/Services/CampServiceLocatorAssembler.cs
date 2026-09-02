
using Galactic1.Code.Gameplay.Construction;
using Galactic1.Code.Gameplay.Noise;
using Galactic1.Code.Gameplay.Units.Brain.Zombie;
using Galactic1.Code.Notification;
using Galactic1.Code.Systems.Interaction;
using Galactic1.Configs;
using Galactic1.Game.UI.Stats;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1
{
    public class CampServiceLocatorAssembler : MonoBehaviour
    {

        [Header("BASIC")] 
        [SerializeField] private HomeMenuButtonsBinder _buttonsBinder;
        [SerializeField] private ViewGameController _viewGameController;

        [Header("GAME")] 
        [SerializeField] private TaskLoggerController _taskLoggerController;
        [SerializeField] private Environment _environment;
        [SerializeField] private ConstructionModeController _constructionModeController;
        
        
        
        

        public void Initialize(DIContainer container)
        {
            var configProvider = container.Resolve<IConfigProvider>();
            
            // BASIC
            ServiceLocator.Current.Register(_buttonsBinder);
            ServiceLocator.Current.Register(new ClassHelper());
            ServiceLocator.Current.Register(new StatViewFactory(configProvider.Get<StatLayoutConfig>()));
            
            var notificationService = new NotificationService(configProvider.Get<NotificationMessageConfig>());
            ServiceLocator.Current.Register<INotificationService>(notificationService);
            
            
            
            // GAME
            ServiceLocator.Current.Register(_environment);
            
            // construction
            ServiceLocator.Current.Register(_constructionModeController);
            
            // zombie swarm
            ServiceLocator.Current.Register(new PackCoordinator());
            ServiceLocator.Current.Register(new NoiseSystem());
            
            
            var sceneServicesClear = new EventBinding<SceneServicesClearEvent>(() =>
            {
                ServiceLocator.Current.Unregister<StatViewFactory>();
                ServiceLocator.Current.Unregister<INotificationService>();
                ServiceLocator.Current.Unregister<HomeMenuButtonsBinder>();
                ServiceLocator.Current.Unregister<ClassHelper>();
                ServiceLocator.Current.Unregister<Environment>();
                
                // construction
                ServiceLocator.Current.Unregister<ConstructionModeController>();
                
                ServiceLocator.Current.Unregister<PackCoordinator>();
                ServiceLocator.Current.Unregister<NoiseSystem>();
            });
            EventBus<SceneServicesClearEvent>.Register(sceneServicesClear);
        }

    }
}
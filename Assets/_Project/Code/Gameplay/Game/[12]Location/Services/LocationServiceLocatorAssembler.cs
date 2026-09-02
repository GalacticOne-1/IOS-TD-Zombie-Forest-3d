
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
    public class LocationServiceLocatorAssembler : MonoBehaviour
    {
        [SerializeField] private Environment _environment;

        public void Initialize(DIContainer container)
        {
            var configProvider = container.Resolve<IConfigProvider>();
            
            ServiceLocator.Current.Register(new StatViewFactory(configProvider.Get<StatLayoutConfig>()));
            
            var notificationService = new NotificationService(configProvider.Get<NotificationMessageConfig>());
            ServiceLocator.Current.Register<INotificationService>(notificationService);
            
            ServiceLocator.Current.Register(_environment);
            
            // zombie swarm
            ServiceLocator.Current.Register(new PackCoordinator());
            ServiceLocator.Current.Register(new NoiseSystem());
            
            
            var sceneServicesClear = new EventBinding<SceneServicesClearEvent>(() =>
            {
                ServiceLocator.Current.Unregister<StatViewFactory>();
                ServiceLocator.Current.Unregister<INotificationService>();
                
                ServiceLocator.Current.Unregister<Environment>();
                
                ServiceLocator.Current.Unregister<PackCoordinator>();
                ServiceLocator.Current.Unregister<NoiseSystem>();
            });
            EventBus<SceneServicesClearEvent>.Register(sceneServicesClear);
        }

    }
}
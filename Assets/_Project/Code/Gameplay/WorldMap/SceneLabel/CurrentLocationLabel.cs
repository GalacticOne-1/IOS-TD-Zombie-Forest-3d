
using Galactic1.Code.Notification;
using Galactic1.Code.Systems.Squad;
using Galactic1.Core.Results;
using UnityEngine;

namespace Galactic1.Code.WorldMap
{
    /// <summary>
    /// Мини-панель над локацией на карте для текущей локации
    /// Создаётся и управляется WorldMapController / NodeManager.
    /// </summary>
    public class CurrentLocationLabel : WorldMapLabelBase
    {
        [Header("UI Elements")] [SerializeField]
        private GameObject enterButton;
        
        

        /// <summary>
        /// Привязать маркер к конкретной ноде.
        /// </summary>
        public override void Bind(MapNode node)
        {
            base.Bind(node);
            
            UpdateMarker(node);

            // Подписка на изменения состояния ноды
            node.OnNodeStateChanged += OnCurrentNodeStateChanged;
        }

        // private void OnDestroy()
        // {
        //     if (boundNode != null)
        //         boundNode.OnNodeStateChanged -= OnNodeStateChanged;
        // }
        
        
        
        

        /// <summary>
        /// Обновление кнопки
        /// </summary>
        public void UpdateMarker(MapNode node)
        {
            if (boundNode == null) return;

            var config = node.Config;

            // для смены сцен
            enterButton.RegisterButtonClick(() =>
            {
                if (config.LocationType == LocationType.Home)
                {
                    EventBus<HomeSceneRequestEvent>.Raise(new HomeSceneRequestEvent());
                }
                else
                {
                    if (ServiceLocator.Current.Get<SquadValidationService>().
                            ValidateForCampDefense() == SquadValidationResult.Success)
                    {
                        EventBus<LocationSceneRequestEvent>.Raise(new LocationSceneRequestEvent{ LocationId = config.Index });
                    }
                    else
                    {
                        ServiceLocator.Current.Get<INotificationService>().Push(NotificationFailReason.SquadIsDestroyed);
                    }
                    
                }
            });
            
            // var locationEnter = ServiceLocator.Current.Get<WorldMapController>().LocationEnter;
            // locationEnter.Attach(
            //     transform.parent.position + offset,
            //     ServiceLocator.Current.Get<IMainCamera>().Camera);
            //
            // locationEnter.gameObject.RegisterButtonClick(() =>
            // {
            //     if (config.LocationType == LocationType.Home)
            //     {
            //         EventBus<HomeSceneRequestEvent>.Raise(new HomeSceneRequestEvent());
            //     }
            //     else
            //     {
            //         EventBus<LocationSceneRequestEvent>.Raise(new LocationSceneRequestEvent
            //             { locationId = config.Index });
            //     }
            // });
        }

        private void OnCurrentNodeStateChanged(MapNode node)
        {
            UpdateMarker(node);
        }
    }
}
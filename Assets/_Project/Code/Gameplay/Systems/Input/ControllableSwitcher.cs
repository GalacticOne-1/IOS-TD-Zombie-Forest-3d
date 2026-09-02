using System;
using Galactic1.Core;
using UnityEngine;
using PlayerRepository = Galactic1.Repository.PlayerRepository;

namespace Galactic1.Gameplay.Control
{
    /*
     *  активирует нужный контроллер
     *  отключает другой
     *  сообщает в InputManager, кто сейчас под управлением
     *  обновляет UI (через UIButtonVisibilityFilter)
     */
    public static class ControllableSwitcher
    {

        public static bool IsDragon { get; private set; }
        
        public static Action OnSwitch;
        

        public static void Restore()
        {
            var activator = new PlayerControlActivator();
            
            var state = ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy;
            var player = ServiceLocator.Current.Get<PlayerRepository>().GetController;
            //var dragon = ServiceLocator.Current.Get<DragonRepository>().GetController;
            
            // IsDragon = state.PlayerUnits[0].OnDragon.CurrentValue;
            // if (IsDragon)
            // {
            //     activator.ActivateDragon();
            // }
            // else
            // {
            //     activator.ActivatePlayerUnit(
            //         new Vector2(player.tr.position.x, 0),
            //         new Vector2(player.tr.position.x, .2f));
            //     // позиция из сохранения ??? -- если игрок закончил игру где то на этажах лагеря
            // }
            // OnSwitch?.Invoke();
        }
        
        
        /// <summary>
        /// Сесть на дракона (for button)
        /// </summary>
        public static void Button_OnDragon()
        {
            var player = ServiceLocator.Current.Get<PlayerRepository>().GetController;
            //var dragon = ServiceLocator.Current.Get<DragonRepository>().GetController;
            //var dist = player.tr.position.MAT_Distance(dragon._groundDetector.tr.position);
            // if (dist < AppConstants.MAX_DISTANCE_TO_DRAGON)
            // {
            //     IsDragon = true;
            //     ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.PlayerUnitData[0].OnDragon.Value = true;
            //     
            //     // // #1 переключаем управление на дракона
            //     new PlayerControlActivator().ActivateDragon();
            //     OnSwitch?.Invoke();
            // }
            // else
            // {
            //     DLog.Alert($"DRAGON_Action : distance to dragon [{dist}]", EDlogColor.ORANGE);
            //     ServiceLocator.Current.Get<NotificationSystem>()
            //         .Spawn($"{ServiceLocator.Current.Get<LocalisationService>().Data.dragon_too_far}".SetText(EDlogColor.ORANGE));
            // }
        }

        /// <summary>
        /// Слезть с дракона (for button)
        /// </summary>
        public static void Button_OutDragon()
        {
            // var dragon = ServiceLocator.Current.Get<DragonRepository>().GetController;
            //
            // // что бы слезть с дракона нужно чекать землю
            // if (dragon._groundDetector.GroundExist())
            // {
            //     dragon._groundDetector.GetDismountPosition(out Vector2 hitPoint, out Vector2 dismountCoord);
            //     if (dismountCoord != Vector2.zero)
            //     {
            //         IsDragon = false;
            //         ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.PlayerUnitData[0].OnDragon.Value = false;
            //         // #1 переключаем управление на дракона
            //         new PlayerControlActivator().ActivatePlayerUnit(hitPoint, dismountCoord);
            //         OnSwitch?.Invoke();
            //     }
            //     else
            //     {
            //         ServiceLocator.Current.Get<NotificationSystem>()
            //             .Spawn($"{ServiceLocator.Current.Get<LocalisationService>().Data.ground_too_far}".SetText(EDlogColor.ORANGE));
            //     }
            // }
            // else
            // {
            //     ServiceLocator.Current.Get<NotificationSystem>()
            //         .Spawn($"{ServiceLocator.Current.Get<LocalisationService>().Data.ground_too_far}".SetText(EDlogColor.ORANGE));
            // }
        }

    }

}
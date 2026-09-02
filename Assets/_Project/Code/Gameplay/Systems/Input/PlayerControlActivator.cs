using System.Collections.Generic;
using Galactic1.Gameplay.Player;
using Galactic1.Repository;
using UnityEngine;

namespace Galactic1.Gameplay.Control
{
    /// <summary>
    /// Единственный класс, который отвечает за полную активацию контроллеров:
    /// - Переход игрок ↔ дракон
    /// - Настройка камеры
    /// - Настройка физики
    /// - Установка позиции
    /// - Установка границ локации
    /// - Обновление UI
    /// </summary>
    public class PlayerControlActivator
    {

        // ------------------ PLAYER UNIT -----------------------
        public void ActivatePlayerUnit(Vector2 ground, Vector2 dismountPosition)
        {
            // #1 переключаем управление на дракона
            //ServiceLocator.Current.Get<PlayerInteractionController>().SetDragonButton(false);
            ServiceLocator.Current.Get<HeroStateMachine>().ChangeState(HeroStateMachine.EPlayerController.Unit);
            
            // var player = ServiceLocator.Current.Get<PlayerRepository>().GetController;
            // var dragon = ServiceLocator.Current.Get<DragonRepository>().GetController;
            //
            // dragon.ControllerDisable();
            // player.ControllerEnable();
            //
            // CameraFollow.I.SetObjectForFollowing(player.gameObject);
            // CameraFollow.I.offset = new Vector3(0, 2, 0);
            // player.GetComponent<Rigidbody2D>().simulated = true;
            // //player.onDragon = false;
            // //new SetUnitAnimationController();
            // player.SwitchAnimatorController();
            //
            // // #2 устанавливаем юнит
            // player.tr.parent = ServiceLocator.Current.Get<Environment>().playerUnits;
            // player.tr.position = dismountPosition;
            // player.tr.rotation = Quaternion.Euler(Vector2.zero);
            //
            //
            //
            // /*
            //  *      Когда юнит меняет этаж или спешивается, он получает настройки земли под ногами
            //  *      таким образом юнит всегда будет находится в границах, на какой бы земле не стоял
            //  *      (этажи в лагере или осторва в локациях)
            //  */
            // // #3 получаем установку по земле
            // ContactFilter2D filtr = new ContactFilter2D();
            // filtr.SetLayerMask(1 << AppConstants.layer_walkable_ground);
            // filtr.useTriggers = true;
            //
            // List<Collider2D> gr = new List<Collider2D>();
            // if (Physics2D.OverlapCircle(ground, .2f, filtr, gr) > 0)
            // {
            //     for (int i = 0; i < gr.Count; i++)
            //     {
            //         //DLog.Alert($"#{i} >> {gr[i]}", EDlogColor.YELLOW);
            //
            //         // *** устанавливаем границы по земле
            //         if (gr[i].GetComponent<IGroundSetup>() != null)
            //         {
            //             var groundSetup = gr[i].GetComponent<IGroundSetup>().GetSetup();
            //             new LOCATION_SETUP().SetGroundBorderX(new Vector2(groundSetup.xMin, groundSetup.xMax));
            //             new LOCATION_SETUP().SetGroundBorderY(new Vector2(groundSetup.y, groundSetup.y));
            //         }
            //     }
            // }
            // //Debug.LogError($"detected ground : {gr.Count}, filtr {filtr.layerMask}");
        }


        // ------------------ DRAGON -----------------------
        public void ActivateDragon()
        {
            // // #1 переключаем управление на дракона
            // //ServiceLocator.Current.Get<PlayerInteractionController>().SetDragonButton(true);
            // ServiceLocator.Current.Get<HeroStateMachine>().ChangeState(HeroStateMachine.EPlayerController.Dragon);
            //
            // var player = ServiceLocator.Current.Get<PlayerRepository>().GetController;
            // var dragon = ServiceLocator.Current.Get<DragonRepository>().GetController;
            //
            // player.ControllerDisable();
            // dragon.ControllerEnable();
            //
            // CameraFollow.I.SetObjectForFollowing(dragon.gameObject);
            // CameraFollow.I.offset = new Vector3(0, 2, 0);
            // player.GetComponent<Rigidbody2D>().simulated = false;
            // //player.onDragon = true;
            // //new SetUnitAnimationController();
            // player.SwitchAnimatorController();
            //
            // // #2 устанавливаем юнит
            // player.tr.parent = dragon.playerPlace.transform;
            // player.tr.localPosition = Vector2.zero;
            //
            // // поворачиваем в ту же сторону что и дракон
            // player.tr.localRotation = Quaternion.Euler(Vector2.zero);
            // player.tr.localScale = new Vector3(-1, 1, 1);
            //
            //
            // /*
            //  *      У дракона всегда одна граница, это сама локация
            //  */
            // // #3 устанавливаем границы
            // new LOCATION_SETUP().SetGroundBorderX(ServiceLocator.Current.Get<GlobalRepository>().LocationBorderX);
            // new LOCATION_SETUP().SetGroundBorderY(ServiceLocator.Current.Get<GlobalRepository>().LocationBorderY);
        }
    }
}

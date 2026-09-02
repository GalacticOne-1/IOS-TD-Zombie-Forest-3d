using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Weapons.Infrastructure;
using Galactic1.Code.Gameplay.Weapons.View;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Equipment_Preview
{
    public sealed class WeaponEquipSystem_Preview
    {
        /// <summary>
        /// Только для превьюшки
        /// </summary>
        public WeaponHandle Equip(
            ISquadMember unit, 
            WeaponModule module,
            EquipmentSlotVisual slotVisual)
        {
            // Собрать Entity через Factory
            var factory = new WeaponFactory(unit.AmmoInventory, unit.StatsProvider);
            var entity = factory.Create(module, module.Definition, null);
            
            // Создать View-префаб и прикрепить к руке
            var viewGo = $"{AppConstants.PATH_ITEMS}{module.Item.PrefabPath}"
                .CreateGO(module.Info.isTwoHanded ? slotVisual.attachment1 : slotVisual.attachment2);
            viewGo.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var view = viewGo.GetComponent<WeaponView>();
            view.Bind(unit.UnitAdapter, entity, module.Definition.ToData());

            entity.Equip();

            var handle = new WeaponHandle(entity, view);
            unit.CurrentWeaponHandle = handle;
            //unit.ReloadHandler.Bind(entity);
            //unit.WeaponSlot.Mount(handle, module.Definition, unit.AnimationController, unit.Animator);
            
            
            return handle;
        }
    }
}
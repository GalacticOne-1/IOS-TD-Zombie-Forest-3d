using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Weapons.Infrastructure;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.View
{
    // ─────────────────────────────────────────────
    //  WeaponEquipSystem — финальная склейка
    //  Создаёт Entity + View, связывает их
    // ─────────────────────────────────────────────

    public sealed class WeaponEquipSystem
    {
        /// <summary>
        /// Выдать оружие weaponId юниту unit.
        /// unit реализует IWeaponOwner — предоставляет инвентарь и статы.
        /// </summary>
        public WeaponHandle Equip(
            ISquadMember unit, 
            WeaponModule module,
            EquipmentSlotVisual slotVisual,
            IInventorySource source,
            EquipmentRuntimeService equipmentService,
            int slotIndex)
        {
            // связка с инвентарем
            var inventorySync = new WeaponInventorySync(source, equipmentService, slotIndex);

            // Собрать Entity через Factory
            var factory = new WeaponFactory(unit.AmmoInventory, unit.StatsProvider);
            var entity = factory.Create(module, module.Definition, inventorySync);
            
            // Создать View-префаб и прикрепить к руке
            var viewGo = $"{AppConstants.PATH_ITEMS}{module.Item.PrefabPath}"
                .CreateGO(module.Info.isTwoHanded ? slotVisual.attachment1 : slotVisual.attachment2);
            viewGo.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            
            // создаем контейнер из конфига
            var weaponDefinition = module.Definition.ToData();
            weaponDefinition.WeaponType = module.Info.weaponType;
            var view = viewGo.GetComponent<WeaponView>();
            view.Bind(unit.UnitAdapter, entity, weaponDefinition);

            entity.Equip();
            inventorySync.Bind(entity);

            var handle = new WeaponHandle(entity, view);
            unit.CurrentWeaponHandle = handle;
            unit.ReloadHandler.Bind(entity);
            unit.WeaponSlot.Mount(handle, module.Definition, unit.AnimationController, unit.Animator);
            
            
            
            // ****************************************************************************************************
            // ****************************************************************************************************
            // 🔥 RESTORE AMMO (в конце когда оружие собрано!!!)
            var slot = source.GetSlot(slotIndex);
            entity.Get<AmmoComponent>()?.RestoreAmmo(slot.AmmoInMagazine);
            entity.Get<DurabilityComponent>()?.RestoreDurability(slot.Durability);
            
            // ****************************************************************************************************
            
            // Восстанавливаем состояние которое было до смены оружия
            // Engaging не восстанавливаем — AI сам переоткроет огонь увидев врага
            var stateToRestore = unit.StateMachine.CurrentStateId;
            var targetState = stateToRestore == UnitStateId.Engaging || stateToRestore == UnitStateId.MeleeEngaging
                ? UnitStateId.Idle
                : stateToRestore;

            unit.StateMachine.Execute(new EquipWeaponCommand(targetState));
            
            return handle;
        }
        
    }
}
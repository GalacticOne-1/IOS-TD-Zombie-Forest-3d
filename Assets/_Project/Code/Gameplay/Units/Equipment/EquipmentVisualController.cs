using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment_Preview;
using Galactic1.Code.Gameplay.Weapon.Animation;
using Galactic1.Code.Gameplay.Weapons.View;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Equipment
{
    /// <summary>
    /// ТОЛЬКО визуалы. Никакой логики инвентаря.
    /// Подписывается на EquipmentRuntimeService события.
    /// </summary>
    public sealed class EquipmentVisualController : MonoBehaviour
    {
        [Header("⚔️ Equipment Visuals (как в LDOE)")]
        public List<EquipmentSlotVisual> visualSlots = new();

        /// Активные модели в сцене
        private Dictionary<EquipSlotType, GameObject> activeModels = new ();

        private ISquadMember survivorInstance;
        private IEquipmentStatsProvider _presentation;
        private WeaponRigController _rigController;

        private IEquipmentVisualHandler _handler;
        
        
        public void Bind(IEquipmentStatsProvider provider, IEquipmentVisualHandler handler)
        {
            _presentation = provider;
            _handler = handler;
            survivorInstance = GetComponent<ISquadMember>();
            _rigController = GetComponent<WeaponRigController>();
            
            handler.Bind(
                visualSlots,
                activeModels,
                survivorInstance,
                _presentation,
                _rigController);
            
            _presentation.OnEquipped += OnEquipped;
            _presentation.OnUnequipped += OnUnequipped;
            
            _presentation.OnClearAll += () =>
            {
                foreach (var kvp in activeModels)
                    if (kvp.Value)
                        Destroy(kvp.Value);
                activeModels.Clear();
            };
        }

        private void OnEquipped(EquipSlotType slot, ItemConfig item)
        {
            var slotIndex = EquipmentUtility.GetSlotType(_presentation.Source, slot);
            if (slotIndex.HasValue)
                _handler.BindVisual(slot, item, slotIndex.Value);
        }

        private void OnUnequipped(EquipSlotType slot)
        {
            _handler.ClearVisual(slot);
        }
        
        
        // public void BindVisual(EquipSlotType slot, ItemConfig item, int slotIndex)
        // {
        //     if (item == null || string.IsNullOrEmpty(item.PrefabPath))
        //         return;
        //
        //     // Найти attach point
        //     var visualDef = visualSlots.Find(v => v.slot == slot);
        //     if (visualDef == null ||
        //         visualDef.attachment1 == null && visualDef.attachment2 == null)
        //         return;
        //
        //     // Удалить существующую модель
        //     ClearVisual(slot);
        //
        //     // *** оружие создается через отдельный сервис
        //     if (slot == EquipSlotType.Weapon)                     
        //     {
        //         // 1. Заспавнить новый префаб на кость руки
        //         WeaponHandle weaponHandle;
        //         if(_previewMode)
        //         {
        //             weaponHandle = new WeaponEquipSystem_Preview().Equip(
        //                 survivorInstance,
        //                 item.Weapon,
        //                 visualDef);
        //         }
        //         else
        //         {
        //             weaponHandle = new WeaponEquipSystem().Equip(
        //                 survivorInstance,
        //                 item.Weapon,
        //                 visualDef,
        //                 _presentation.Source,
        //                 (EquipmentRuntimeService)_presentation,
        //                 slotIndex);
        //         }
        //         activeModels[slot] = weaponHandle.View.gameObject;
        //         
        //         // 2. Взять Grip точки с префаба
        //         var gripPoints = weaponHandle.View.gameObject.GetComponent<WeaponGripPoints>();
        //
        //         // 3. Передать Target в IK и переключить Rig
        //         _rigController.AttachWeapon(item.Weapon.Info.weaponType, gripPoints);
        //         
        //         // 4. Переключить анимации
        //         survivorInstance.AnimationController.SetWeapon(item.Weapon.Info.weaponType);
        //         return;
        //     }
        //
        //
        //     // *** для всего остального
        //
        //     // Создать новую
        //     var prefab = Resources.Load<GameObject>($"{AppConstants.PATH_ITEMS}{item.PrefabPath}");
        //     if (prefab == null || visualDef.attachment1 == null)
        //     {
        //         Debug.LogError("Equipment not created!");
        //         return;
        //     }
        //
        //     GameObject model = Instantiate(prefab, visualDef.attachment1);
        //     model.transform.localPosition = Vector3.zero;
        //     model.transform.localRotation = Quaternion.identity;
        //     model.transform.localScale = Vector3.one;
        //
        //     activeModels[slot] = model;
        // }
        //
        // public void ClearVisual(EquipSlotType slot)
        // {
        //     if (activeModels.TryGetValue(slot, out var model) && model != null)
        //     {
        //         // отдельно снимаем оружие
        //         if (slot == EquipSlotType.Weapon)
        //         {
        //             _rigController.DetachWeapon();
        //             survivorInstance.ReloadHandler.Interrupt(survivorInstance.CurrentWeaponHandle.Entity);
        //             survivorInstance.CurrentWeaponHandle.Dispose();
        //             survivorInstance.WeaponSlot.Unmount();
        //             survivorInstance.AnimationController.SetWeapon(WeaponType.Unarmed);
        //         }
        //         else
        //             Destroy(model);
        //     }
        //
        //     activeModels.Remove(slot);
        // }

        /// <summary>
        /// вкл\выкл модель
        /// </summary>
        /// <param name="slot"></param>
        public void SwitchVisual(EquipSlotType slot)
        {
            if (activeModels.TryGetValue(slot, out var model))
                model?.SetActive(!model.activeSelf);
        }
    }
}
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Weapon.Animation;
using Galactic1.Code.Gameplay.Weapons.View;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Equipment_Preview
{

    public interface IEquipmentVisualHandler
    {
        void Bind(
            List<EquipmentSlotVisual> visualSlots,
            Dictionary<EquipSlotType, GameObject> activeModels,
            ISquadMember survivorInstance,
            IEquipmentStatsProvider presentation,
            WeaponRigController rigController);
        
        void BindVisual(EquipSlotType slot, ItemConfig item, int slotIndex);
        void ClearVisual(EquipSlotType slot);
    }
    
    public class EquipmentVisualHandler : IEquipmentVisualHandler
    {
        private List<EquipmentSlotVisual> _visualSlots;

        private Dictionary<EquipSlotType, GameObject> _activeModels = new ();

        private ISquadMember _survivorInstance;
        private IEquipmentStatsProvider _presentation;
        private WeaponRigController _rigController;
        
        public void Bind(
            List<EquipmentSlotVisual> visualSlots,
            Dictionary<EquipSlotType, GameObject> activeModels,
            ISquadMember survivorInstance,
            IEquipmentStatsProvider presentation,
            WeaponRigController rigController)
        {
            _visualSlots = visualSlots;
            _activeModels = activeModels;
            _survivorInstance = survivorInstance;
            _presentation = presentation;
            _rigController = rigController;
        }

        public void BindVisual(EquipSlotType slot, ItemConfig item, int slotIndex)
        {
            if (item == null || string.IsNullOrEmpty(item.PrefabPath))
                return;
        
            // Найти attach point
            var visualDef = _visualSlots.Find(v => v.slot == slot);
            if (visualDef == null ||
                visualDef.attachment1 == null && visualDef.attachment2 == null)
                return;
        
            // Удалить существующую модель
            ClearVisual(slot);
        
            // *** оружие создается через отдельный сервис
            if (slot == EquipSlotType.Weapon)                     
            {
                // 1. Заспавнить новый префаб на кость руки
                WeaponHandle weaponHandle;
                weaponHandle = new WeaponEquipSystem().Equip(
                    _survivorInstance,
                    item.Weapon,
                    visualDef,
                    _presentation.Source,
                    (EquipmentRuntimeService)_presentation,
                    slotIndex);
                _activeModels[slot] = weaponHandle.View.gameObject;
                
                // 2. Взять Grip точки с префаба
                var gripPoints = weaponHandle.View.gameObject.GetComponent<WeaponGripPoints>();
        
                // 3. Передать Target в IK и переключить Rig
                _rigController.AttachWeapon(item.Weapon.Info.weaponType, gripPoints);
                
                // 4. Переключить анимации
                _survivorInstance.AnimationController.SetWeapon(item.Weapon.Info.weaponType);
                return;
            }
        
        
            // *** для всего остального
        
            // Создать новую
            var prefab = Resources.Load<GameObject>($"{AppConstants.PATH_ITEMS}{item.PrefabPath}");
            if (prefab == null || visualDef.attachment1 == null)
            {
                Debug.LogError("Equipment not created!");
                return;
            }
        
            GameObject model = prefab.CreateGO(visualDef.attachment1);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
        
            _activeModels[slot] = model;
        }
        
        public void ClearVisual(EquipSlotType slot)
        {
            if (_activeModels.TryGetValue(slot, out var model) && model != null)
            {
                // отдельно снимаем оружие
                if (slot == EquipSlotType.Weapon)
                {
                    _rigController.DetachWeapon();
                    _survivorInstance.ReloadHandler.Interrupt(_survivorInstance.CurrentWeaponHandle.Entity);
                    _survivorInstance.CurrentWeaponHandle.Dispose();
                    _survivorInstance.WeaponSlot.Unmount();
                    _survivorInstance.AnimationController.SetWeapon(WeaponType.Unarmed);
                }
                else
                    GameObject.Destroy(model);
            }
        
            _activeModels.Remove(slot);
        }
    }
}
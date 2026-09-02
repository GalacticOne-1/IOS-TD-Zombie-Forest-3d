
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Audio.Weapons;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.UI.Inventory;
using Galactic1.Game.Meta.Items;
using Galactic1.Systems;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.Services
{
    public sealed class WeaponReloadService
    {
        private readonly InventoryController _controller;
        private readonly AmmoRegistry _ammoRegistry;

        public WeaponReloadService(
            InventoryController controller,
            AmmoRegistry ammoRegistry)
        {
            _controller = controller;
            _ammoRegistry = ammoRegistry;
        }

        // =========================
        // RELOAD
        // =========================
        public int Reload(InventoryView view, int weaponIndex)
        {
            // оружие из снаряжения юнита  !!!
            var rightSource = _controller.RightSource;
            var weaponSlot = rightSource.GetSlot(weaponIndex);

            if (weaponSlot.IsEmpty)
                return 0;

            var weapon = weaponSlot.Item;
            var def = weapon.Weapon.Definition;

            int missing = def.magazineSize - weaponSlot.AmmoInMagazine;
            if (missing <= 0)
                return 0;

            var ammoType = def.supportedAmmo;
            int loaded = 0;

            // слоты для поиска патронов из левого источника (camp/transport)!!!
            var leftSource = _controller.LeftSource;
            var slots = leftSource.GetSlots();

            for (int i = 0; i < slots.Count && missing > 0; i++)
            {
                var slot = slots[i];
                if (slot.IsEmpty || !slot.Item.HasModule<AmmoModule>())
                    continue;

                var ammo = slot.Item.Ammo;

                if (ammo.AmmoType != ammoType)
                    continue;

                int take = Mathf.Min(slot.Amount, missing);

                slot.Amount -= take;
                missing -= take;
                loaded += take;

                if (slot.Amount <= 0)
                    leftSource.ClearSlot(i);
                else
                    leftSource.SetSlot(i, slot);
            }

            if (loaded > 0)
            {
                weaponSlot.AmmoInMagazine += loaded;
                rightSource.SetSlot(weaponIndex, weaponSlot);
                
                // sound
                // EventBus<WeaponAudioEvent>.Raise(new WeaponAudioEvent( // для этого события нужны классы из рейда !!
                //     Vector3.one, 
                //     def.audio.ToData(),
                //     WeaponAudioEventType.ReloadComplete));
                AudioService.Play(AudioService.Player.Command.WeaponReloadClip);
                
                // ui toast
                Vector3? slotPosition = view?.selectedSlot.gameObject.CMP_RectTr().position;
                if (slotPosition.HasValue)
                {
                    ServiceLocator.Current.Get<FloatingTextService>().ShowText(
                        slotPosition.Value,
                        $"+{loaded} {GameContent.Ammo.GetByCaliber(def.supportedAmmo.Id)[0].Header.titleLid}",
                        Color.white);
                }
            }

            return loaded;
        }

        // =========================
        // UNLOAD
        // =========================
        public int Unload(InventoryView view, int weaponIndex)
        {
            var rightSource = _controller.RightSource;
            var weaponSlot = rightSource.GetSlot(weaponIndex);

            if (weaponSlot.IsEmpty)
                return 0;

            var weaponItem = weaponSlot.Item;
            var def = weaponItem.Weapon.Definition;

            int ammoCount = weaponSlot.AmmoInMagazine;
            if (ammoCount <= 0)
                return 0;

            
            var leftSource = _controller.LeftSource;
            var ammoType = def.supportedAmmo;

            // просто находим подходящий конфиг для патронов этого калибра
            var configs = _ammoRegistry.GetByCaliber(ammoType.Id);
            if (configs == null || configs.Count == 0)
                return 0;

            // разгрузка оружия 
            // по схеме "сколько смогло влезть в инвентарь, остальное остается в оружии"
            var result = leftSource.TryAdd(
                new InventorySlotRuntime(
                    configs[0], 
                    ammoCount, 
                    0, 
                    0));
            
            weaponSlot.AmmoInMagazine = result.Remaining;
            rightSource.SetSlot(weaponIndex, weaponSlot);
            
            var unloadAmmo = ammoCount - result.Remaining;
            
            // sound
            AudioService.Play(AudioService.Player.Command.WeaponUnloadClip);
            
            // ui toast
            Vector3? slotPosition = view?.selectedSlot.gameObject.CMP_RectTr().position;
            if (slotPosition.HasValue)
            {
                ServiceLocator.Current.Get<FloatingTextService>().ShowText(
                    slotPosition.Value,
                    $"-{unloadAmmo} {GameContent.Ammo.GetByCaliber(def.supportedAmmo.Id)[0].Header.titleLid}",
                    Color.white);
            }

            return unloadAmmo;
        }

    }
}
using System;
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Weapons.Infrastructure;
using Galactic1.Code.Gameplay.Weapons.View;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    public sealed class WeaponSlot : MonoBehaviour
    {
        public IWeaponWithEvents CurrentWeapon { get; private set; }
        public WeaponAnimBridge AnimBridge { get; private set; }
        
        public event Action<WeaponHandle> OnWeaponMounted;
        public event Action OnWeaponUnmounted;
        
        

        /// <summary>Вызывается из WeaponEquipSystem при выдаче оружия юниту.</summary>
        public void Mount(
            WeaponHandle handle,
            WeaponDefinition weaponDef,
            UnitAnimationController animController,
            Animator animator)
        {
            CurrentWeapon = new WeaponEntityAdapter(handle);

            AnimBridge = new WeaponAnimBridge(
                weaponDef,
                animController,
                animator);
            AnimBridge.Bind(handle.Entity);
            //GetComponentInChildren<CombatAnimHandler>().WeaponAnimBridge = AnimBridge;
            GetComponentInChildren<CombatAnimHandler>().Bind(AnimBridge);
            
            // ==============================
            // FIRE SPEED
            // ==============================
            float clipLength = 1.1f;
            float rpmInterval = 60f / weaponDef.roundsPerMinute;
            float speed = clipLength / rpmInterval;
            animator.SetFloat("FireSpeed", speed);
            
            // ==============================
            // RELOAD SPEED
            // ==============================
            // ⚠️ длина reload-клипа (надо знать точно!)
            float reloadDuration = weaponDef.reloadTimeSec;
            animator.SetFloat("ReloadSpeedIdle", 3.1f / reloadDuration);
            animator.SetFloat("ReloadSpeedMove", 4f / reloadDuration);
            animator.SetFloat("ReloadSpeedRun", 3.2f / reloadDuration);
            
            
            // ***************************************************************************************************
            OnWeaponMounted?.Invoke(handle);
        }

        public void Unmount()
        {
            OnWeaponUnmounted?.Invoke();
            AnimBridge?.Unbind();
            AnimBridge = null;
            CurrentWeapon = null;
        }
    }
}
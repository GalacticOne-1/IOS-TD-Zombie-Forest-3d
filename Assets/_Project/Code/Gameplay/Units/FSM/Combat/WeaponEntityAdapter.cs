using System;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Code.Gameplay.Weapons.View;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    /// <summary>
    /// Адаптирует WeaponEntity к контракту IWeaponWithEvents,
    /// который ожидают EngagingState и ReloadingState.
    /// </summary>
    public sealed class WeaponEntityAdapter : IWeaponWithEvents
    {
        private readonly WeaponHandle _handle;
        private readonly WeaponEntity _runtime;
        public WeaponEntity Entity => _runtime;
        
        
        
        // ── IWeaponWithEvents ──
        public WeaponState State => _runtime.State;
        public WeaponDefinitionData Definition => _runtime.Definition;
        public int CurrentAmmo => _runtime.Get<AmmoComponent>()?.CurrentAmmo ?? 0;
        public int ClipSize => _runtime.Get<AmmoComponent>()?.ClipSize ?? 0;
        
        public bool CanFire => _runtime.CanFire;
        
        // ── Data ──
        public float Durability01 =>  _durabilityMax > 0 ? (float)_durability / _durabilityMax : 0f;
        public int Durability => Mathf.CeilToInt(((float)_durability / _durabilityMax) * 100);
        
        
        public event Action OnShotLogicComplete;
        public event Action<WeaponState> OnStateChanged;
        public event Action OnReloadCompleted;
        public event Action<int, float> OnDurabilityChanged;
        public event Action<int, int> OnAmmoChanged;
        
        private int _durability;
        private int _durabilityMax;
        

        public WeaponEntityAdapter(WeaponHandle handle)
        {
            _handle = handle;
            _runtime = handle.Entity;

            // Пробрасываем события напрямую
            _runtime.OnShotLogicComplete += () => OnShotLogicComplete?.Invoke();
            _runtime.OnStateChanged += s => OnStateChanged?.Invoke(s);
            _runtime.OnReloadCompleted += () => OnReloadCompleted?.Invoke();
            
            _runtime.OnAmmoChanged += (cur, max) => OnAmmoChanged?.Invoke(cur, max);
            
            _runtime.OnDurabilityChanged += OnDurabilityInternal;
        }


        
        private void OnDurabilityInternal(int cur, int max)
        {
            _durability = cur;
            _durabilityMax = max;
           
            OnDurabilityChanged?.Invoke(Durability, Durability01);
        }


        public void SetVisible(bool visible) => _handle.SetVisible(visible);

        public void Fire(FireContext context) => _runtime.RequestFire(context);
        public void Reload() => _runtime.RequestReload();
        
    }
}
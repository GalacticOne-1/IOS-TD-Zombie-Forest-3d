
using System;
using Galactic1.Code.Gameplay.Combat.Data;
using Galactic1.Code.Gameplay.Weapons.Infrastructure;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    public sealed class WeaponEntity
    {
        public WeaponModule Module { get; }
        public WeaponDefinitionData Definition { get; }
        public IWeaponInventorySync InventorySync { get; }

        public WeaponState State { get; private set; } = WeaponState.Unequipped;

        public bool CanFire => State == WeaponState.Ready
                               && (Get<FireComponent>()?.IsReady ?? true);

        public bool CanReload
        {
            get
            {
                if (State == WeaponState.Reloading || State == WeaponState.Unequipped)
                    return false;

                var l = _components.Count;
                for (int i = 0; i < l; i++)
                    if (!_components[i].CanReload(this))
                        return false;

                return true;
            }
        }

        private readonly System.Collections.Generic.List<IWeaponComponent> _components = new(8);
        private readonly System.Collections.Generic.Dictionary<Type, IWeaponComponent> _lookup = new(8);

        public event Action<WeaponState> OnStateChanged;
        public event Action<FireRequest> OnFired; // View: спавнит снаряд/луч — fired AFTER combat resolves
        public event Action<int, int> OnDurabilityChanged;
        public event Action OnNoAmmo;
        public event Action OnReloadRequested;
        public event Action OnReloadStarted;
        public event Action OnReloadCanceled;
        public event Action OnReloadCompleted;
        public event Action OnOverheated;
        public event Action<int, int> OnAmmoChanged;
        public event Action OnShotLogicComplete;
        public event Action OnFireAnimationRequested;

        // <<< NEW — raised by FireComponent INSTEAD of calling NotifyFired directly.
        // WeaponCombatBridge subscribes to this and owns the combat-then-visuals sequencing.
        public event Action<FireRequest> OnCombatFireRequested;

        public WeaponEntity(WeaponModule module, WeaponDefinitionData definition, IWeaponInventorySync inventorySync)
        {
            Module = module;
            Definition = definition;
            InventorySync = inventorySync;
        }

        public void AddComponent(IWeaponComponent component)
        {
            _components.Add(component);
            _lookup[component.GetType()] = component;
        }

        public T Get<T>() where T : class, IWeaponComponent
        {
            _lookup.TryGetValue(typeof(T), out var c);
            return c as T;
        }

        public bool Has<T>() where T : class, IWeaponComponent
            => _lookup.ContainsKey(typeof(T));

        public void Equip()
        {
            foreach (var c in _components) 
                c.OnEquip(this);
            SetState(WeaponState.Ready);
        }

        public void Unequip()
        {
            OnStateChanged = null;
            OnFired = null;
            OnDurabilityChanged = null;
            OnNoAmmo = null;
            OnReloadRequested = null;
            OnReloadStarted = null;
            OnReloadCanceled = null;
            OnReloadCompleted = null;
            OnOverheated = null;
            OnAmmoChanged = null;
            OnShotLogicComplete = null;
            OnFireAnimationRequested = null;
            OnCombatFireRequested = null; // <<< NEW — clear like every other event on unequip

            foreach (var c in _components) 
                c.OnUnequip();
            
            SetState(WeaponState.Unequipped);
            InventorySync?.Unbind(this);
        }

        public void Tick(float deltaTime)
        {
            foreach (var c in _components) 
                c.Tick(deltaTime);
        }

        /// <summary>
        /// Точка входа от AI/Targeting.
        /// context.TargetDistance — дистанция до цели, уже посчитанная снаружи.
        /// WeaponEntity не знает кто цель и как она выбрана — только "стреляем на N метров".
        /// </summary>
        public void RequestFire(FireContext context)
        {
            if (!CanFire)
                return;
 
            foreach (var c in _components)
                c.OnFireRequested(this);
 
            if (!CanFire)
                return;
 
            Get<FireComponent>()?.Execute(this, context);
        }

        // <<< REMOVED CALL SITE NOTE:
        // FireComponent.OnAnimationFireEvent() used to call entity.NotifyFired(request)
        // directly. It now calls entity.RaiseCombatFireRequested(request) instead.
        // NotifyFired remains internal and is only called from CompleteFire() below.
        internal void NotifyFired(FireRequest request)
        {
            foreach (var c in _components) 
                c.OnFireExecuted(this);
            OnFired?.Invoke(request);
        }

        public void RequestReload()
        {
            if (!CanReload) 
                return;
            Get<ReloadComponent>()?.StartReload(this);
        }

        internal void SetState(WeaponState s)
        {
            if (State == s) 
                return;
            State = s;
            OnStateChanged?.Invoke(s);
        }

        internal void RaiseFireAnimationRequested() => OnFireAnimationRequested?.Invoke();
        internal void RaiseNoAmmo() => OnNoAmmo?.Invoke();
        internal void RaiseReloadRequested() => OnReloadRequested?.Invoke();
        internal void RaiseReloadStarted() => OnReloadStarted?.Invoke();
        internal void RaiseReloadCanceled() => OnReloadCanceled?.Invoke();
        internal void RaiseReloadCompleted() => OnReloadCompleted?.Invoke();
        internal void RaiseOverheated() => OnOverheated?.Invoke();
        internal void RaiseAmmoChanged(int cur, int max) => OnAmmoChanged?.Invoke(cur, max);
        internal void RaiseDurabilityChanged(int val, int max) => OnDurabilityChanged?.Invoke(val, max);
        internal void RaiseShotLogicComplete() => OnShotLogicComplete?.Invoke();

        // <<< NEW — called by FireComponent.OnAnimationFireEvent() instead of NotifyFired.
        // Hands control to WeaponCombatBridge via the OnCombatFireRequested subscription.
        internal void RaiseCombatFireRequested(FireRequest request)
            => OnCombatFireRequested?.Invoke(request);

        // <<< NEW — single completion point for one fire action.
        // Called ONLY by WeaponCombatBridge, AFTER WeaponFireService.Execute()
        // has fully resolved gameplay (DamagePipeline, suppression, events).
        //
        // Order matters:
        //   1. FireComponent.CompleteFire — applies cooldown (weapon runtime concern)
        //   2. NotifyFired — WeaponView plays projectile/tracer/hitscan visuals
        //   3. RaiseShotLogicComplete — EngagingState FSM unlocks _isFiring
        internal void CompleteFire(FireRequest request)
        {
            Get<FireComponent>()?.CompleteFire(this);

            NotifyFired(request);
            RaiseShotLogicComplete();
        }
    }
}
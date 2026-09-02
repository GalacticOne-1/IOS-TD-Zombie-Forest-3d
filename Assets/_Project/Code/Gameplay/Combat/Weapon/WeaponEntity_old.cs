using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Combat.Data;
using Galactic1.Code.Gameplay.Weapons.Infrastructure;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{

    // ─────────────────────────────────────────────
    //  WeaponEntity — ядро, хранит состояние и компоненты
    // ─────────────────────────────────────────────

    public sealed class WeaponEntity_old
    {
        // Данные из SO (только чтение после инициализации)
        // public WeaponModule Module { get; }
        // public WeaponDefinitionData Definition { get; }
        // public IWeaponInventorySync InventorySync {get; }
        //
        // // Состояние
        // public WeaponState State { get; private set; } = WeaponState.Unequipped;
        // public bool CanFire => State == WeaponState.Ready 
        //                        && (Get<FireComponent>()?.IsReady ?? true);
        //
        // public bool CanReload
        // {
        //     get
        //     {
        //         if (State == WeaponState.Reloading || State == WeaponState.Unequipped)
        //             return false;
        //
        //         var l = _components.Count;
        //         for (int i = 0; i < l; i++)
        //             if (!_components[i].CanReload(this)) return false;
        //
        //         return true;
        //     }
        // }
        //
        //
        //
        // // Компоненты (добавляются через WeaponFactory)
        // private readonly List<IWeaponComponent> _components = new(8);
        // private readonly Dictionary<Type, IWeaponComponent> _lookup = new(8);
        //
        //
        //
        // // События — View подписывается на них
        // public event Action<WeaponState> OnStateChanged;
        // public event Action<FireRequest> OnFired; // View: спавнит снаряд/луч
        // public event Action<int, int> OnDurabilityChanged;
        // public event Action OnNoAmmo;
        // public event Action OnReloadRequested;
        // public event Action OnReloadStarted;
        // public event Action OnReloadCanceled;
        // public event Action OnReloadCompleted;
        // public event Action OnOverheated;
        // public event Action<int, int> OnAmmoChanged; // (current, clipSize)
        // public event Action OnShotLogicComplete;
        // public event Action OnFireAnimationRequested;
        //
        //
        // public WeaponEntity(WeaponModule module, WeaponDefinitionData definition, IWeaponInventorySync inventorySync)
        // {
        //     Module = module;
        //     Definition = definition;
        //     InventorySync = inventorySync;
        //
        //     //OnNoAmmo += () => DLog.Alert("No Ammo", EDlogColor.ORANGE); // todo сдeлать тост нал юнитом
        // }
        //
        //
        // // ── Компоненты ──
        //
        // public void AddComponent(IWeaponComponent component)
        // {
        //     _components.Add(component);
        //     _lookup[component.GetType()] = component;
        // }
        //
        // public T Get<T>() where T : class, IWeaponComponent
        // {
        //     _lookup.TryGetValue(typeof(T), out var c);
        //     return c as T;
        // }
        //
        // public bool Has<T>() where T : class, IWeaponComponent
        //     => _lookup.ContainsKey(typeof(T));
        //
        // // ── Lifecycle ──
        //
        // public void Equip()
        // {
        //     foreach (var c in _components) c.OnEquip(this);
        //     SetState(WeaponState.Ready);
        // }
        //
        // public void Unequip()
        // {
        //     OnStateChanged = null;
        //     OnFired = null;
        //     OnDurabilityChanged = null;
        //     OnNoAmmo = null;
        //     OnReloadRequested = null;
        //     OnReloadStarted = null;
        //     OnReloadCanceled = null;
        //     OnReloadCompleted = null;
        //     OnOverheated = null;
        //     OnAmmoChanged = null;
        //     OnShotLogicComplete = null;
        //     OnFireAnimationRequested = null;
        //     
        //     foreach (var c in _components) c.OnUnequip();
        //     SetState(WeaponState.Unequipped);
        //     InventorySync?.Unbind(this);
        // }
        //
        // // Вызывается WeaponTimerSystem каждый кадр — один раз на всех
        // public void Tick(float deltaTime)
        // {
        //     foreach (var c in _components) c.Tick(deltaTime);
        // }
        //
        // // ── Действия ──
        //
        // public void RequestFire()
        // {
        //     if (!CanFire) return;
        //
        //     foreach (var c in _components) c.OnFireRequested(this);
        //     // Компоненты могут сменить State внутри (например AmmoComp → Empty)
        //     if (!CanFire) return;
        //
        //     // FireComponent строит FireRequest и вызывает Execute
        //     Get<FireComponent>()?.Execute(this);
        // }
        //
        // // Вызывается FireComponent после расчёта выстрела
        // internal void NotifyFired(FireRequest request)
        // {
        //     foreach (var c in _components) c.OnFireExecuted(this);
        //     OnFired?.Invoke(request);
        //     Debug.Log("[WeaponEntity] NotifyFired called");
        // }
        //
        // public void RequestReload()
        // {
        //     if (!CanReload) return;
        //     Get<ReloadComponent>()?.StartReload(this);
        // }
        //
        //
        //
        // // ── Уведомления от компонентов (internal API) ──
        //
        // internal void SetState(WeaponState s)
        // {
        //     if (State == s) return;
        //     State = s;
        //     OnStateChanged?.Invoke(s);
        // }

        

        // internal void RaiseFireAnimationRequested() => OnFireAnimationRequested?.Invoke();
        // internal void RaiseNoAmmo() => OnNoAmmo?.Invoke();
        // internal void RaiseReloadRequested() => OnReloadRequested?.Invoke();
        // internal void RaiseReloadStarted() => OnReloadStarted?.Invoke();
        // internal void RaiseReloadCanceled() => OnReloadCanceled?.Invoke();
        // internal void RaiseReloadCompleted() => OnReloadCompleted?.Invoke();
        // internal void RaiseOverheated() => OnOverheated?.Invoke();
        // internal void RaiseAmmoChanged(int cur, int max) => OnAmmoChanged?.Invoke(cur, max);
        // internal void RaiseDurabilityChanged(int val, int max) => OnDurabilityChanged?.Invoke(val, max);
        // internal void RaiseShotLogicComplete() => OnShotLogicComplete?.Invoke();
    }
}
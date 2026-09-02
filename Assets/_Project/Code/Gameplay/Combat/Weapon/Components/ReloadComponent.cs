using System.Collections.Generic;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    public sealed class ReloadComponent : WeaponComponentBase
    {
        private float _reloadTimer;
        private bool _reloading;

        public override void Tick(float dt)
        {
            if (!_reloading) return;
            _reloadTimer -= dt;
            if (_reloadTimer <= 0f)
                CompleteReload();
        }

        public void StartReload(WeaponEntity entity)
        {
            var ammo = entity.Get<AmmoComponent>();
            if (ammo != null && ammo.CurrentAmmo == ammo.ClipSize) return;
            if (!entity.Get<FireComponent>().IsReady) return;

            _reloading = true;
            _reloadTimer = entity.Definition.ReloadTimeSec;
            entity.SetState(WeaponState.Reloading);
            entity.RaiseReloadStarted();

            _pendingEntity = entity;
        }

        public void Interrupt(WeaponEntity entity)
        {
            if (!_reloading) return;
            _reloading = false;
            _pendingEntity = null;
            // Патроны не добавляются — перезарядка прервана
            entity.SetState(entity.Get<AmmoComponent>()?.CurrentAmmo > 0
                ? WeaponState.Ready
                : WeaponState.Empty);
            entity.RaiseReloadCanceled();
        }

        private WeaponEntity _pendingEntity;

        // оружие завершило перезарядку
        private void CompleteReload()
        {
            _reloading = false;
            if (_pendingEntity == null) return;

            _pendingEntity.RaiseReloadCompleted();
            foreach (var c in GetComponents(_pendingEntity))
                c.OnReloadCompleted(_pendingEntity);

            _pendingEntity.SetState(WeaponState.Ready);
            _pendingEntity = null;
            DLog.Alert(">>> Weapon reloading complete <<<");
        }

        // Хелпер — не идеально, но избегает рефлексии
        private static IEnumerable<IWeaponComponent> GetComponents(WeaponEntity e)
        {
            var ammo = e.Get<AmmoComponent>();
            if (ammo != null) yield return ammo;
        }
    }
}
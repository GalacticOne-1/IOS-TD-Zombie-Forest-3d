using Galactic1.Code.Gameplay.Weapons.Logic;

namespace Galactic1.Code.Gameplay.Units
{
    public sealed class UnitReloadHandler
    {
        private readonly SurvivorInstance _unit;
        private bool _isReloading;

        public bool IsReloading => _isReloading;


        public UnitReloadHandler(SurvivorInstance unit)
        {
            _unit = unit;
        }




        public void Bind(WeaponEntity unit)
        {
            unit.OnReloadRequested += () => TryStartReload(unit);
        }


        public void Tick(float dt)
        {
            if (!_isReloading) return;

            // Оружие само тикает и вызовет OnReloadCompleted —
            // мы подписались в StartReload, ждём коллбэк.
        }

        public bool TryStartReload(WeaponEntity entity)
        {
            var weapon = _unit.WeaponSlot.CurrentWeapon;
            if (weapon == null || _isReloading)
                return false;

            var ammo = entity.Get<AmmoComponent>();
            if (ammo != null && !ammo.CanReload(entity))
                return false;

            _isReloading = true;
            weapon.OnReloadCompleted += OnReloadDone;
            weapon.Reload();

            return true;
        }

        public void Interrupt()
        {
            if (_unit.WeaponSlot.CurrentWeapon != null)
                Interrupt(_unit.WeaponSlot.CurrentWeapon.Entity);
        }

        public void Interrupt(WeaponEntity entity)
        {
            if (!_isReloading) return;
            _isReloading = false;

            var weapon = _unit.WeaponSlot.CurrentWeapon;
            if (weapon != null)
            {
                weapon.OnReloadCompleted -= OnReloadDone;
                entity.Get<ReloadComponent>()?.Interrupt(entity);
            }
        }

        private void OnReloadDone()
        {
            _isReloading = false;
            var weapon = _unit.WeaponSlot.CurrentWeapon;
            if (weapon != null)
                weapon.OnReloadCompleted -= OnReloadDone;

            DLog.Alert(">>> Unit reload done (parallel) <<<", AppConstants.show_log_unit_fsm);

            // Если была ожидающая атака — возобновить
            //_unit.ReactiveAI.OnReloadCompleted();
        }
    }
}
namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    public sealed class AmmoComponent : WeaponComponentBase
    {
        private readonly IAmmoInventory _inventory;
        private WeaponEntity _entity;

        public int CurrentAmmo { get; private set; }
        public int ClipSize => _entity?.Definition.MagazineSize ?? 0;


        public AmmoComponent(IAmmoInventory inventory)
        {
            _inventory = inventory;
        }
        
        public int PeekInventoryAmmo()
        {
            var ammoId = _entity?.Definition.SupportedAmmo?.Id;
            if (ammoId == null) return 0;
            return _inventory.PeekAmmo(ammoId);
        }
        
        public override bool CanReload(WeaponEntity entity)
        {
            var ammoDefinition = entity.Definition.SupportedAmmo?.Id;
            if (ammoDefinition == null) return true; // оружие без патронов — не наше дело
    
            // Магазин полный — незачем
            if (CurrentAmmo >= entity.Definition.MagazineSize) return false;
    
            // Патронов нет нигде
            if (_inventory.PeekAmmo(ammoDefinition) <= 0)
            {
                entity.RaiseNoAmmo(); // заглушка для UI
                return false;
            }
    
            DLog.Alert("Ammo found");
            return true;
        }

        public override void OnEquip(WeaponEntity entity)
        {
            _entity = entity;
        }

        public void RestoreAmmo(int ammo)
        {
            // === оружие не имеет аммо, запрос на перезарядку
            if (ammo <= 0)
            {
                _entity.SetState(WeaponState.Empty);

                // авто-старт перезарядки
                if (_entity.CanReload)
                {
                    _entity.RaiseReloadRequested();
                }
                return;
            }

            // === Нормальное восстановление
            Reload(ammo);
            _entity.RaiseAmmoChanged(CurrentAmmo - 1, ClipSize);

#if UNITY_EDITOR
            DLog.Alert($"Ammo restored: {CurrentAmmo}");
#endif
        }

        public override void OnFireRequested(WeaponEntity entity)
        {
            if (CurrentAmmo <= 0)
            {
                entity.SetState(WeaponState.Empty);
                return;
            }

            CurrentAmmo--;
      
#if UNITY_EDITOR
            DLog.Alert($"Ammo changed to {CurrentAmmo}/{ClipSize}");
#endif
            // делаем -1 для правильного отображения в UI
            if (CurrentAmmo - 1 >= 0)
                entity.RaiseAmmoChanged(CurrentAmmo - 1, ClipSize);
            
            if (CurrentAmmo == 0)
                entity.SetState(WeaponState.Empty);
        }

        public override void OnReloadCompleted(WeaponEntity entity)
        {
            var ammoId = entity.Definition.SupportedAmmo?.Id;
            if (ammoId == null) return;

            int needed = entity.Definition.MagazineSize - CurrentAmmo;
            int taken  = _inventory.TakeAmmo(ammoId, needed);
            Reload(taken);
            
            entity.RaiseAmmoChanged(CurrentAmmo - 1, ClipSize);
        }

        // +1 т.к без этого съедается последний патрон (требуется переделать стрельбу в WeaponEntity)
        void Reload(int val) => CurrentAmmo = val + 1;
    }
}
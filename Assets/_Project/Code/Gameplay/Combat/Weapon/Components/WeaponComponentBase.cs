namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    
    // Базовая реализация — переопределяй только нужные методы
    public abstract class WeaponComponentBase : IWeaponComponent
    {
        public virtual void OnEquip(WeaponEntity entity) { }
        public virtual void OnUnequip() { }
        public virtual void Tick(float deltaTime) { }
        public virtual bool CanReload(WeaponEntity entity) => true;
        public virtual void OnFireRequested(WeaponEntity entity) { }
        public virtual void OnFireExecuted(WeaponEntity entity) { }
        public virtual void OnReloadStarted(WeaponEntity entity) { }
        public virtual void OnReloadCompleted(WeaponEntity entity) { }
    }
}
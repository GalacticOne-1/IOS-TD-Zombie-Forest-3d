namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    
    // ─────────────────────────────────────────────
    //  Интерфейс компонента
    // ─────────────────────────────────────────────

    public interface IWeaponComponent
    {
        void OnEquip(WeaponEntity entity);
        void OnUnequip();
        void Tick(float deltaTime);         // вызывается WeaponTimerSystem
        bool CanReload(WeaponEntity entity);
        void OnFireRequested(WeaponEntity entity);  // до выстрела — может отменить
        void OnFireExecuted(WeaponEntity entity);   // после выстрела
        void OnReloadStarted(WeaponEntity entity);
        void OnReloadCompleted(WeaponEntity entity);
    }
}
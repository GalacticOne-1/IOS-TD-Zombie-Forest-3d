using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Combat.Data;
using Galactic1.Code.Gameplay.Weapons.Infrastructure;
using Galactic1.Code.Gameplay.Weapons.Logic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.View
{

    // ─────────────────────────────────────────────
    //  WeaponAnimBridge — управляет Animator юнита
    //  Каждый тип оружия задаёт свои хэши в инспекторе —
    //  никакого switch по типу.
    // ─────────────────────────────────────────────

    public sealed class WeaponAnimBridge : IWeaponAnimationReceiver
    {
        // Назначаются в инспекторе на префабе оружия
        [Header("Animator Parameter Names")]
        [SerializeField] private string boolIsHeavy = ""; // "" = не используется
        [SerializeField] private int weaponTypeInt = 0; // 0=pistol,1=rifle,2=heavy

        public UnitAnimationController AnimController { get; }
        private Animator _animator;
        private WeaponEntity _entity;
        private FireMode _fireMode;
        private int _hashWeaponType;
        private int _hashIsHeavy;

        


        public WeaponAnimBridge(
            WeaponDefinition weaponDef,
            UnitAnimationController animController,
            Animator animator)
        {
            _fireMode = weaponDef.fireMode;
            _animator = animator;
            AnimController = animController;
        }

        public void Bind(WeaponEntity entity)
        {
            _entity = entity;

            _hashWeaponType = Animator.StringToHash("weaponType");

            if (!string.IsNullOrEmpty(boolIsHeavy))
                _hashIsHeavy = Animator.StringToHash(boolIsHeavy);

            // Установить тип оружия — меняет stance и layer weights
            _animator.SetInteger(_hashWeaponType, weaponTypeInt);
            if (_hashIsHeavy != 0)
                _animator.SetBool(_hashIsHeavy, true);

            //entity.OnFired += OnFired;
            _entity.OnFireAnimationRequested += OnFireAnimationRequested;
            _entity.OnReloadStarted += OnReloadStarted;
            _entity.OnReloadCanceled += OnReloadCanceled;
            _entity.OnStateChanged += OnStateChanged;
        }

        public void Unbind()
        {
            if (_entity == null)
                return;

            //_entity.OnFired -= OnFired;
            _entity.OnFireAnimationRequested -= OnFireAnimationRequested;
            _entity.OnReloadStarted -= OnReloadStarted;
            _entity.OnReloadCanceled -= OnReloadCanceled;
            _entity.OnStateChanged -= OnStateChanged;

            StopFiring();

            if (_animator != null && _hashIsHeavy != 0)
                _animator.SetBool(_hashIsHeavy, false);

            _entity = null;
        }

        private void OnFired(FireRequest _)
        {
            if (_fireMode == FireMode.FullAuto)
                AnimController?.SetFiring(true);
            else
                AnimController?.PlayRangedAttack();
        }
        
        // Запускает анимацию — без урона
        private void OnFireAnimationRequested()
        {
            if (_fireMode == FireMode.FullAuto)
                AnimController?.SetFiring(true);
            else
                AnimController?.PlayRangedAttack();
        }

        // Animation Event на клипе Fire Single — фактический выстрел
        public void AE_DoShot()
            => _entity?.Get<FireComponent>()?.OnAnimationFireEvent();

        public void OnGrenadeFinish()
            => AnimController.EndGrenadeToss();


        private void OnReloadStarted() => AnimController?.PlayReload();
        private void OnReloadCanceled() => AnimController?.CancelReload();

        private void OnStateChanged(WeaponState state)
        {
            // Оружие перестало стрелять — сбросить isFiring
            if (state != WeaponState.Ready)
            {
                AnimController?.SetFiring(false);
                AnimController?.CombatExit();
            }
        }

        // Вызывается из EngagingState когда цель потеряна или команда движения
        public void StopFiring()
        {
            AnimController?.SetFiring(false);
            
            // *** выход без сглаживания между клипами
            if (_fireMode != FireMode.FullAuto)
                AnimController?.CombatExit();
        }
    }
}
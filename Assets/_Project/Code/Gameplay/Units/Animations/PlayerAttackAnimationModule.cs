
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation
{
    /// <summary>
    /// Управляет анимацией атаки.
    /// Поддерживает комбо для кулаков (3 удара),
    /// одиночный выстрел для пистолета и винтовки.
    /// </summary>
    public sealed class PlayerAttackAnimationModule : 
        MonoBehaviour,
        IAttackAnimationModule
    {
        [SerializeField] private float comboCooldown = 0.8f;

        private PlayerAnimConfig _config;
        private AnimatorBridge _bridge;
        private WeaponAnimSwitcher _weaponSwitcher;

        // Комбо-счётчик для кулаков
        private int _comboStep;
        private float _lastAttackTime = -999f;
        private const int FistsComboCount = 3;

        public void Initialize(BaseAnimConfig config)
        {
            _config = config as PlayerAnimConfig;
            _bridge = GetComponent<AnimatorBridge>();
            _weaponSwitcher = GetComponent<WeaponAnimSwitcher>();
        }

        /// <summary>
        /// Вызывается из CombatSystem при команде атаки.
        /// </summary>
        public void PlayAttack()
        {
            switch (_weaponSwitcher.CurrentWeapon)
            {
                case WeaponType.Unarmed:
                    PlayMeleeAttack();
                    break;

                default:
                    PlayRangedAttack();
                    return;
            }
        }

        // =========================
        // Private
        // =========================
        public void PlayMeleeAttack()
        {
            // Сбрасываем комбо если пауза слишком долгая
            if (Time.time - _lastAttackTime > comboCooldown)
                _comboStep = 0;

            _bridge.SetFloat(_config.AttackIndexHash, _comboStep, 0f);
            _bridge.SetTrigger(_config.AttackHash);

            _lastAttackTime = Time.time;
            _comboStep = (_comboStep + 1) % FistsComboCount;
        }

        public void PlayRangedAttack()
        {
            // Для оружия дальнего боя используем существующий ShootHash
            //_bridge.SetTrigger(config.ShootHash);
            _bridge.CrossFade("Fire Single", 0.05f, 2, 0f);
        }

        public void ResetState()
        {
            _comboStep = 0;
            _lastAttackTime = -999f;
        }
    }
}
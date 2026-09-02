using Galactic1.Code.Gameplay.Animation.Zombie;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Systems.Squad;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation.Player
{
    /// <summary>
    /// Standard player locomotion animation updater.
    /// </summary>
    [RequireComponent(typeof(AnimatorBridge))]
    [RequireComponent(typeof(UnitMover))]
    public sealed class PlayerLocomotionAnimationModule : MonoBehaviour, ILocomotionAnimationModule
    {
        private AnimatorBridge _bridge;
        private UnitMover _mover;
        private PlayerAnimConfig _config;

        public void Initialize(BaseAnimConfig config, UnitGameplayDefinition definition)
        {
            _config = config as PlayerAnimConfig;

            _bridge = GetComponent<AnimatorBridge>();
            _mover = GetComponent<UnitMover>();
        }

        public void Tick()
        {
            UpdateLocomotion();
        }
        
        private void UpdateLocomotion()
        {
            bool isMoving = _mover.IsMoving;
            _bridge.SetBool(_config.IsMovingHash, isMoving);

            float actualSpeed = _mover.Velocity.magnitude;
            _bridge.SetFloat(_config.SpeedHash, actualSpeed);
        }
    }
}

using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation.Player
{
    public class PlayerCombatAnimationModule : MonoBehaviour, ICombatAnimationModule
    {
        private AnimatorBridge _bridge;
        private PlayerAnimConfig _config;

        public void Initialize(BaseAnimConfig config)
        {
            _config = config as PlayerAnimConfig;
            _bridge = GetComponent<AnimatorBridge>();
        }

        public void PlayShoot() 
            => _bridge.SetTrigger(_config.ShootHash);
        
        public void SetFiring(bool v) 
            => _bridge.SetBool(_config.IsFiringHash, v);
        
        public void PlayReload() 
            => _bridge.SetTrigger(_config.ReloadHash);

        public void CancelReload()
        {
            _bridge.CrossFade(_config.IdleStateName, 0.1f, 0);
            _bridge.PlayState("Empty", 1);
        }

        public void PlayInteract() 
            => _bridge.SetTrigger(_config.InteractHash);
    }
}
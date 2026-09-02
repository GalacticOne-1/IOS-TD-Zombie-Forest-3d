
using Galactic1.Code.Gameplay.Effect;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation.Player
{
    public class PlayerAbilityAnimationModule : MonoBehaviour, IAbilityAnimationModule
    {
        private IWeaponAnimationModule _weaponAnimation;
        private AnimatorBridge _bridge;
        private PlayerAnimConfig _config;


        public void Initialize(BaseAnimConfig config)
        {
            _config = config as PlayerAnimConfig;
            _weaponAnimation = GetComponent<IWeaponAnimationModule>();
            _bridge = GetComponent<AnimatorBridge>();
        }
        
        
        
        public void OnAbilityAnimation(ItemUseContext ctx)
        {
            switch (ctx.AnimationType)
            {
                case AbilityAnimationType.TossGrenade:
                    PlayTossGrenade();
                    break;
            }
        }
        
        
        void PlayTossGrenade()
        {
            //if (_isDead) return;
            
            _weaponAnimation?.SetRigEnabled(false);
            _weaponAnimation?.SetWeaponVisible(false);
            _bridge.SetTrigger(_config.GrenadeHash);
        }
        
        public void EndGrenadeToss()
        {
            //if (_isDead)
                //return;

            // без корутины баг !
            ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait1(() => _weaponAnimation?.SetRigEnabled(true));
            _weaponAnimation?.SetWeaponVisible(true);
        }
    }
}
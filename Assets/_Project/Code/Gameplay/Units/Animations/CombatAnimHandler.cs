
using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation
{
    /// <summary>
    /// Общий модуль событий в анимации
    /// </summary>
    public class CombatAnimHandler : MonoBehaviour
    {
        private IWeaponAnimationReceiver _weapon;
        private IMeleeAnimationReceiver _melee;
        private IAbilityAnimationReceiver _ability;

        public void Bind(object target)
        {
            if (target is IWeaponAnimationReceiver w)
                _weapon = w;

            if (target is IMeleeAnimationReceiver m)
                _melee = m;

            if (target is IAbilityAnimationReceiver a)
                _ability = a;
        }
        
        
        
        public void AE_MeleeHit()
            => _melee.OnAnimationMeleeHitEvent();
        
        public void AE_MeleeFinished()
            => _melee.OnAnimationFinished();
        
        public void AE_DoShot()
            => _weapon?.AE_DoShot();
        
        public void AE_TossGrenade() 
            => _ability?.ExecutePending();

        public void AE_TossGrenadeFinish()
        {
            _weapon?.OnGrenadeFinish();
            _ability?.OnAbilityFinished();
        }
    }
}
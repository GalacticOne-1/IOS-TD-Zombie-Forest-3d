using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation
{
    [CreateAssetMenu(
        fileName = "PlayerAnimConfig",
        menuName  = "Game Configs/Player/Player Anim Config")]
    public sealed class PlayerAnimConfig : BaseAnimConfig
    {
        [Header("Combat")]
        public string attackTrigger     = "Attack";
        public string attackIndexParam  = "AttackIndex";
        public string isAimingParam     = "IsAiming";
        public string shootTrigger      = "Shoot";
        public string isFiringParam     = "IsFiring";
        public string reloadTrigger     = "Reload";
        public string isGrenadeTrigger  = "IsGrenade";
        public string isInCoverParam    = "IsInCover";

        [Header("Actions")]
        public string interactTrigger = "Interact";

        [System.NonSerialized] public int AttackHash;
        [System.NonSerialized] public int AttackIndexHash;
        [System.NonSerialized] public int IsAimingHash;
        [System.NonSerialized] public int ShootHash;
        [System.NonSerialized] public int IsFiringHash;
        [System.NonSerialized] public int ReloadHash;
        [System.NonSerialized] public int GrenadeHash;
        [System.NonSerialized] public int IsInCoverHash;
        [System.NonSerialized] public int InteractHash;

        protected override void OnEnable()
        {
            base.OnEnable();

            AttackHash       = Animator.StringToHash(attackTrigger);
            AttackIndexHash  = Animator.StringToHash(attackIndexParam);
            IsAimingHash     = Animator.StringToHash(isAimingParam);
            ShootHash        = Animator.StringToHash(shootTrigger);
            IsFiringHash     = Animator.StringToHash(isFiringParam);
            ReloadHash       = Animator.StringToHash(reloadTrigger);
            GrenadeHash      = Animator.StringToHash(isGrenadeTrigger);
            IsInCoverHash    = Animator.StringToHash(isInCoverParam);
            InteractHash     = Animator.StringToHash(interactTrigger);
        }
    }
}
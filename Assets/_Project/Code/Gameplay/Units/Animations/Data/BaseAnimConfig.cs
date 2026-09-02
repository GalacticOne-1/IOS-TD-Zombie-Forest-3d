using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation
{
    public abstract class BaseAnimConfig : ScriptableObject
    {
        [Header("Variants")] 
        public string variantParam = "Variant";
        public string idleSelectState = "Idle_Select";
        public string walkSelectState = "Walk_Select";
        public string runSelectState = "Run_Select";
        public string dieSelectState = "Death_Select";
        public string attackSelectState = "Attack_Select";

        [Header("Locomotion")] 
        public string IdleStateName = "Idle";
        public string speedParam = "Speed";
        public string isMovingParam = "IsMoving";
        public string isCrouchingParam = "IsCrouching";

        [Header("Actions")] 
        public string hitTrigger = "Hit";
        public string dieTrigger = "Die";
        public string panicParam = "IsPanicking";

        [System.NonSerialized] public int VariantHash;
        [System.NonSerialized] public int IdleSelectHash;
        [System.NonSerialized] public int WalkSelectHash;
        [System.NonSerialized] public int RunSelectHash;
        [System.NonSerialized] public int DieSelectHash;
        [System.NonSerialized] public int AttackSelectHash;
        [System.NonSerialized] public int SpeedHash;
        [System.NonSerialized] public int IsMovingHash;
        [System.NonSerialized] public int IsCrouchingHash;
        [System.NonSerialized] public int HitHash;
        [System.NonSerialized] public int DieHash;
        [System.NonSerialized] public int PanicHash;

        protected virtual void OnEnable()
        {
            VariantHash = Animator.StringToHash(variantParam);
            IdleSelectHash = Animator.StringToHash(idleSelectState);
            WalkSelectHash = Animator.StringToHash(walkSelectState);
            RunSelectHash = Animator.StringToHash(runSelectState);
            DieSelectHash = Animator.StringToHash(dieSelectState);
            AttackSelectHash = Animator.StringToHash(attackSelectState);
            SpeedHash = Animator.StringToHash(speedParam);
            IsMovingHash = Animator.StringToHash(isMovingParam);
            IsCrouchingHash = Animator.StringToHash(isCrouchingParam);
            HitHash = Animator.StringToHash(hitTrigger);
            DieHash = Animator.StringToHash(dieTrigger);
            PanicHash = Animator.StringToHash(panicParam);
        }
    }
}
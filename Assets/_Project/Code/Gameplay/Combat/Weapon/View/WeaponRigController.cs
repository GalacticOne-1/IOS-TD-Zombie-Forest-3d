
using Galactic1.Core.Enums;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Galactic1.Code.Gameplay.Weapon.Animation
{
    public sealed class WeaponRigController : MonoBehaviour
    {
        [SerializeField] private Transform hitOrigin;
        [SerializeField] private Rig weaponRig;
        [SerializeField] private TwoBoneIKConstraint rightHandIK;
        [SerializeField] private TwoBoneIKConstraint leftHandIK;

        private RigBuilder rigBuilder;

        public Transform HitOrigin => hitOrigin;

        private float leftHandIKWeight;
        

        private void Awake()
        {
            rigBuilder = GetComponentInChildren<RigBuilder>();
        }
        
        
        
        /// <summary>
        /// Управление IK/ригом оружия.
        /// </summary>
        public void SetRigEnabled(bool enabled)
        {
            weaponRig.weight = enabled ? 1f : 0f;

            if (!enabled)
            {
                rightHandIK.weight = 0f;
                leftHandIK.weight = 0f;
            }
            else
            {
                rightHandIK.weight = 1f;
                leftHandIK.weight = leftHandIKWeight;
            }

            rigBuilder.Build();
        }

        public void AttachWeapon(WeaponType weaponType, WeaponGripPoints gripPoints)
        {
            rightHandIK.data.target = gripPoints.RightHandGrip;
            rightHandIK.weight = 1f;

            leftHandIK.data.target = gripPoints.LeftHandGrip;
            leftHandIK.weight = gripPoints.LeftHandWeight;
            leftHandIKWeight = gripPoints.LeftHandWeight;
            
            weaponRig.weight = weaponType == WeaponType.Unarmed ? 0 : 1;
            
            rigBuilder.Build();
        }

        public void DetachWeapon()
        {
            rightHandIK.data.target = null;
            leftHandIK.data.target = null;
            weaponRig.weight = 0f;
            rigBuilder.Build();
        }
    }
}
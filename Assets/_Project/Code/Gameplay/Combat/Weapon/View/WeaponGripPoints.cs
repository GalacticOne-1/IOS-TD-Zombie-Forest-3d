using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapon.Animation
{
    /// <summary>
    /// Хранит точки хвата на префабе оружия.
    /// Назначаются вручную в Inspector префаба.
    /// </summary>
    public sealed class WeaponGripPoints : MonoBehaviour
    {
        [SerializeField] private Transform rightHandGrip;
        [SerializeField] private Transform leftHandGrip;
        [Range(0f, 1f)] 
        [SerializeField] private float leftHandWeight = .5f;

        public Transform RightHandGrip => rightHandGrip;
        public Transform LeftHandGrip => leftHandGrip;

        public float LeftHandWeight => leftHandWeight;
    }
}
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    /// <summary>
    /// Управляет всеми hitbox коллайдерами юнита
    /// </summary>
    public sealed class UnitHitboxController : MonoBehaviour
    {
        private Collider[] _colliders;

        private void Awake()
        {
            _colliders = GetComponentsInChildren<Collider>(true);
        }

        public void SetEnabled(bool value)
        {
            for (int i = 0; i < _colliders.Length; i++)
                _colliders[i].enabled = value;
        }
    }
}
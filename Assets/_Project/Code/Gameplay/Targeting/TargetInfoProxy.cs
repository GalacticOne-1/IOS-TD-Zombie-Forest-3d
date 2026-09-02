using Galactic1.Code.Core.Lifecycle;
using Galactic1.Code.Gameplay.Units;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Targeting
{
    public sealed class TargetInfoProxy : MonoBehaviour, IEntityProxy
    {
        [SerializeField] private Collider[] _colliders;

        public int Priority => 0;

        private ITargetInfo _target;
        private bool _registered;

        public void Bind(ITargetInfo target)
        {
            _target = target;
        }

        public void Register()
        {
            if (_registered || _target == null)
                return;

            for (int i = 0; i < _colliders.Length; i++)
            {
                TargetInfoRegistry.Register(_colliders[i], _target);
            }

            _registered = true;
        }

        public void Unregister()
        {
            if (!_registered)
                return;

            for (int i = 0; i < _colliders.Length; i++)
            {
                TargetInfoRegistry.Unregister(_colliders[i]);
            }

            _registered = false;
        }

        private void OnDestroy()
        {
            Unregister();
        }
    }
}
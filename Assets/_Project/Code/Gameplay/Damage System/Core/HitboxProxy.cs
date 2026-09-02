
using Galactic1.Code.Core.Lifecycle;
using Galactic1.Code.Gameplay.Combat.Data;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Damage
{
    /// <summary>
    /// Прокси на коллайдере.
    /// Связывает hitbox → unit runtime.
    /// </summary>
    public sealed class HitboxProxy : MonoBehaviour, IEntityProxy
    {
        public int Priority => 10;
        
        [Tooltip("Body zone this collider represents. Used by BodyPartModifierStep.")]
        public BodyPartType BodyPart = BodyPartType.Torso;
        
        private DamageReceiverProxy _receiver;
        private Collider _collider;
        private bool _registered;
        
        public DamageReceiverProxy Receiver => _receiver;

        
        
        public void Bind()
        {
            _receiver = GetComponentInParent<DamageReceiverProxy>();
            _collider = GetComponent<Collider>();
        }
        
        public void Register()
        {
            if (_registered || _collider == null || _receiver == null)
                return;

            HitboxRegistry.Register(_collider, _receiver);
            _registered = true;
        }

        public void Unregister()
        {
            if (!_registered)
                return;

            HitboxRegistry.Unregister(_collider);
            _registered = false;
        }
        
        
        private void OnDestroy()
        {
            Unregister();
        }
        
    }

}
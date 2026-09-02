using Galactic1.Code.Gameplay.Damage;
using Galactic1.Code.Systems.Raid;
using Galactic1.Game.Meta.Items;
using Galactic1.PoolObject;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.View
{
    /// <summary>
    /// PooledProjectile — снаряд (hit detection only).
    /// НЕ содержит логики урона или FX.
    /// </summary>
    public abstract class BaseProjectile :
        PoolableMonoBehaviour,
        IPoolItemConfig<ItemConfig>,
        IUpdate
    {
        [Header("Ballistics")]
        [SerializeField] private float speed = 40f;
        [SerializeField] private float maxLifetime = 3f;

        // ─────────────────────────────────────────────
        // Config
        // ─────────────────────────────────────────────

        private ItemConfig _itemConfig;

        // ─────────────────────────────────────────────
        // Runtime
        // ─────────────────────────────────────────────

        protected ISceneUnit _attacker;
        protected float _damage;
        private float _armorPiercing;

        private float _lifetime;
        private Vector3 _velocity;
        protected bool _launched;

        // ─────────────────────────────────────────────
        // Pool config
        // ─────────────────────────────────────────────

        public void SetConfig(ItemConfig config)
        {
            _itemConfig = config;
        }

        // ─────────────────────────────────────────────
        // Pool lifecycle
        // ─────────────────────────────────────────────

        public override void OnSpawn()
        {
            base.OnSpawn();
            _launched = false;
            _lifetime = 0f;
        }

        public override void OnDespawn()
        {
            _launched = false;
            _attacker = null;
            _velocity = Vector3.zero;
            _lifetime = 0f;
            _damage = 0f;
            _armorPiercing = 0f;

            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
            base.OnDespawn();
        }

        public override void ResetState()
        {
            base.ResetState();
            _launched = false;
            _velocity = Vector3.zero;
            _lifetime = 0f;
        }

        // ─────────────────────────────────────────────
        // Launch
        // ─────────────────────────────────────────────

        public void Launch(
            ISceneUnit attacker,
            Vector3 direction,
            float damage,
            float armorPiercing)
        {
            _attacker = attacker;
            _damage = damage;
            _armorPiercing = armorPiercing;

            _velocity = direction.normalized * speed;
            _lifetime = maxLifetime;
            _launched = true;

            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
        }

        // ─────────────────────────────────────────────
        // Update
        // ─────────────────────────────────────────────

        public void IUpdateClear()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }

        public void UpdateM()
        {
            if (!IsSpawned || !_launched) return;

            float dt = Time.deltaTime;

            _lifetime -= dt;
            if (_lifetime <= 0f)
            {
                ReturnToPool();
                return;
            }

            transform.position += _velocity * dt;
        }

        // ─────────────────────────────────────────────
        // Trigger fallback
        // ─────────────────────────────────────────────

        private void OnTriggerEnter(Collider other)
        {
            if (!_launched) return;

            ProcessHit(other, transform.position, -_velocity.normalized);
        }

        // ─────────────────────────────────────────────
        // Hit processing
        // ─────────────────────────────────────────────

        protected virtual void ProcessHit(Collider collider, Vector3 point, Vector3 normal)
        {
            if (!IsSpawned || !_launched) return;

            if (collider.TryGetComponent<HitboxProxy>(out var proxy))
            {
                var receiver = proxy.Receiver;

                if (receiver == null || receiver.Unit == _attacker)
                {
                    ReturnToPool();
                    return;
                }

                if (!TeamService.CanDamage(_attacker.Runtime, receiver.Unit?.RuntimeBase))
                    return;

                var hitInfo = new HitInfo
                {
                    Point = point,
                    Normal = normal,
                    Collider = collider,
                    Transform = collider.transform
                };

                DamageResolver.Apply(
                    receiver,
                    _attacker,
                    _damage,
                    DamageType.Bullet,
                    hitInfo);
            }

            ReturnToPool();
        }

    }
}
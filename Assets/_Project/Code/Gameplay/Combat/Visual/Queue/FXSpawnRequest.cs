using Galactic1.Code.GameDatabase.Registries;
using Galactic1.PoolObject;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Visual
{
    /// <summary>
    /// Deferred FX spawn command.
    /// Stored in AsyncFXSpawnQueue and executed frame-budgeted.
    ///
    /// FIXED vs initial Phase 4 draft:
    /// - VfxId now imported from Galactic1.Code.GameDatabase.Registries
    ///   (matches CombatSurfaceFXConfig.ImpactFXId / DecalId field type).
    ///
    /// Used by AsyncFXSpawnQueue.
    /// </summary>
    public readonly struct FXSpawnRequest
    {
        private readonly VfxId _effectId;
        private readonly Vector3 _position;
        private readonly Quaternion _rotation;
        private readonly float _duration;

        public FXSpawnRequest(
            VfxId effectId,
            Vector3 position,
            Quaternion rotation,
            float duration = 0f)
        {
            _effectId = effectId;
            _position = position;
            _rotation = rotation;
            _duration = duration;
        }

        /// <summary>
        /// Dispatches the spawn request through EffectRequestSystem.
        /// Call from AsyncFXSpawnQueue.LateUpdateM() each frame.
        /// </summary>
        public void Execute(EffectRequestSystem effectSystem)
        {
            if (effectSystem == null)
                return;

            effectSystem.Request(
                new EffectRequest
                {
                    Id = _effectId,
                    Position = _position,
                    Rotation = _rotation,
                    Duration = _duration > 0f ? _duration : 0f
                },
                EffectPriority.Normal,
                fx => fx.gameObject.SetActive(true));
        }
    }
}
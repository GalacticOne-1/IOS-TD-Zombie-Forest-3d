
using UnityEngine;

namespace Galactic1.Core.Gameplay
{
    /// <summary>
    /// Безопасный доступ к слоям.
    /// Убирает прямую зависимость от ScriptableObject.
    /// </summary>
    public sealed class LayerService : IGameService
    {
        private readonly LayerConfig _config;

        public LayerService(LayerConfig config)
        {
            _config = config;
            Layers.Setup(this);
        }

        
        // === Perception ===
        public LayerMask Detectable => _config.Detectable;
        public LayerMask Damageable => _config.DamageableAll;
        public LayerMask Occlusion => _config.Occlusion;
        

        // === Combat ===
        public LayerMask BulletHit => _config.BulletHit;
        public LayerMask ExplosionHit => _config.ExplosionHit;

    }
}
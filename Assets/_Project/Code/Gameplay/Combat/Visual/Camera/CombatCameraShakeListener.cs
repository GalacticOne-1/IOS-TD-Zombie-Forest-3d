
namespace Galactic1.Code.Gameplay.Combat.Visual
{
    /// <summary>
    /// Bridges global EventBus combat events
    /// into runtime camera shake service.
    ///
    /// RESPONSIBILITY:
    /// - subscribes to explosion events
    /// - subscribes to suppression events
    /// - forwards events into CombatCameraShakeService
    ///
    /// CONNECTED TO:
    /// - EventBus<ExplosionVisualEvent>
    /// - EventBus<SuppressionVisualEvent>
    /// - CombatCameraShakeService
    ///
    /// IMPORTANT:
    /// Must be disposed/unregistered manually.
    /// </summary>
    public sealed class CombatCameraShakeListener
    {
        private readonly CombatCameraShakeService _shake;

        private readonly EventBinding<ExplosionVisualEvent> _explosionBinding;

        private readonly EventBinding<SuppressionVisualEvent> _suppressionBinding;

        public CombatCameraShakeListener(CombatCameraShakeService shake)
        {
            _shake = shake;

            // =====================================
            // EXPLOSION
            // =====================================

            _explosionBinding = new EventBinding<ExplosionVisualEvent>(OnExplosion);

            EventBus<ExplosionVisualEvent>.Register(_explosionBinding);

            // =====================================
            // SUPPRESSION
            // =====================================

            _suppressionBinding = new EventBinding<SuppressionVisualEvent>(OnSuppression);

            EventBus<SuppressionVisualEvent>.Register(_suppressionBinding);
        }

        /// <summary>
        /// Manual cleanup.
        /// </summary>
        public void Dispose()
        {
            EventBus<ExplosionVisualEvent>.Deregister(_explosionBinding);

            EventBus<SuppressionVisualEvent>.Deregister(_suppressionBinding);
        }

        // =========================================
        // EVENTS
        // =========================================

        private void OnExplosion(ExplosionVisualEvent e)
        {
            _shake.AddExplosionShake(
                e.Position,
                e.Radius,
                e.Intensity,
                lowFrequency: 4f,
                highFrequency: 24f);
        }

        private void OnSuppression(SuppressionVisualEvent e)
        {
            _shake.AddSuppressionShake(e.Intensity);
        }
    }
}
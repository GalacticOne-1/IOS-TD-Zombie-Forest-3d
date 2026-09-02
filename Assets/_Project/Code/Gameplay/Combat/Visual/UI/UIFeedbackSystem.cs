using Galactic1.Code.Gameplay.Combat.Events;

namespace Galactic1.Code.Gameplay.Combat.UI
{
    /// <summary>
    /// Combat gameplay → UI feedback bridge.
    ///
    /// Subscribes to authoritative gameplay events and dispatches
    /// UI update commands (floating damage numbers, kill indicators,
    /// suppression markers).
    ///
    /// RULE:
    ///   UI reads from gameplay events only.
    ///   Never reads from visual events (those are async and throttled).
    ///
    /// Inject IUIFeedbackPresenter to decouple from concrete UI system.
    ///
    /// Lifecycle:
    ///   Created at raid init.
    ///   Disposed at raid end.
    /// </summary>
    public sealed class UIFeedbackSystem
    {
        private readonly IUIFeedbackPresenter _presenter;

        private readonly EventBinding<CombatHitEvent> _hitBinding;
        private readonly EventBinding<CombatDeathEvent> _deathBinding;
        private readonly EventBinding<CombatSuppressionEvent> _suppressionBinding;

        public UIFeedbackSystem(IUIFeedbackPresenter presenter)
        {
            _presenter = presenter;

            _hitBinding = new EventBinding<CombatHitEvent>(OnHit);
            _deathBinding = new EventBinding<CombatDeathEvent>(OnDeath);
            _suppressionBinding = new EventBinding<CombatSuppressionEvent>(OnSuppression);

            EventBus<CombatHitEvent>.Register(_hitBinding);
            EventBus<CombatDeathEvent>.Register(_deathBinding);
            EventBus<CombatSuppressionEvent>.Register(_suppressionBinding);
        }

        public void Dispose()
        {
            EventBus<CombatHitEvent>.Deregister(_hitBinding);
            EventBus<CombatDeathEvent>.Deregister(_deathBinding);
            EventBus<CombatSuppressionEvent>.Deregister(_suppressionBinding);
        }

        // ── Handlers ─────────────────────────────────────────────────

        private void OnHit(CombatHitEvent e)
        {
            _presenter?.ShowDamageNumber(e.Point, e.Damage, e.BodyPart);
        }

        private void OnDeath(CombatDeathEvent e)
        {
            _presenter?.ShowKillIndicator(e.Point);
        }

        private void OnSuppression(CombatSuppressionEvent e)
        {
            if (e.Target == null) return;
            _presenter?.ShowSuppressionMarker(e.Target, e.Amount);
        }
    }
}
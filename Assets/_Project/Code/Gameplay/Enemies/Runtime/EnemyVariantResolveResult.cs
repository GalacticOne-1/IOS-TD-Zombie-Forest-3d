
namespace Galactic1.Code.Gameplay.Enemies.Variants
{
    public readonly struct EnemyVariantResolveResult
    {
        public readonly VariantResolveStatus Status;

        public readonly string SelectedVariantId;       // <= вариант темы

        public readonly string ResolvedVisualPrefabId;  // <= префаб скина

        public readonly string DiagnosticReason;

        private EnemyVariantResolveResult(
            VariantResolveStatus status,
            string selectedVariantId,
            string resolvedVisualPrefabId,
            string diagnosticReason)
        {
            Status = status;
            SelectedVariantId = selectedVariantId;
            ResolvedVisualPrefabId = resolvedVisualPrefabId;
            DiagnosticReason = diagnosticReason;
        }

        public static EnemyVariantResolveResult Resolved(
            string variantId,
            string visualPrefabId)
        {
            return new(
                VariantResolveStatus.Resolved,
                variantId,
                visualPrefabId,
                string.Empty);
        }

        public static EnemyVariantResolveResult FallbackUsed(
            string variantId,
            string visualPrefabId,
            string reason)
        {
            return new(
                VariantResolveStatus.FallbackUsed,
                variantId,
                visualPrefabId,
                reason);
        }

        public static EnemyVariantResolveResult DefaultRequired(
            string reason)
        {
            return new(
                VariantResolveStatus.DefaultRequired,
                string.Empty,
                string.Empty,
                reason);
        }
    }
}
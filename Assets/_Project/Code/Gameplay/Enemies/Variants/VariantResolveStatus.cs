namespace Galactic1.Code.Gameplay.Enemies.Variants
{
    public enum VariantResolveStatus
    {
        /// <summary>Найден тематический вариант.</summary>
        Resolved,

        /// <summary>Тематические не подошли — использован FallbackVariant.</summary>
        FallbackUsed,

        /// <summary>Ни вариантов, ни fallback — использовать базовый PrefabId из конфига.</summary>
        DefaultRequired
    }
}
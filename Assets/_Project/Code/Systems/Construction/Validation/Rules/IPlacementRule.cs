namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Правило проверки размещения здания.
    /// Каждое правило проверяет одно условие.
    /// </summary>
    public interface IPlacementRule
    {
        PlacementValidationResult Validate(PlacementValidationContext context);
    }
}
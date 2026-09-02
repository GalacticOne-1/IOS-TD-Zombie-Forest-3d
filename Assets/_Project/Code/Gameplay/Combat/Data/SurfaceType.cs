namespace Galactic1.Code.Gameplay.Combat.Data
{
    /// <summary>
    /// Physical surface category.
    /// Used by:
    /// - SurfaceResolver
    /// - SurfaceMaterialDatabase
    /// - FX systems
    /// - Penetration systems
    /// </summary>
    public enum SurfaceType
    {
        Default,
        Metal,
        Concrete,
        Wood,
        Organic,
        Glass
    }
}
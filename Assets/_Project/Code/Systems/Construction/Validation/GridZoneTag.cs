namespace Galactic1.Code.Gameplay.Grid
{
    /// <summary>
    /// Тип статической зоны сетки.
    /// Используется для проверки, разрешено ли конкретному зданию
    /// строиться в зоне с данным тегом.
    /// </summary>
    public enum GridZoneTag
    {
        None = 0,
        Locked = 1,
        Main = 5,
        Defense = 10
    }
}
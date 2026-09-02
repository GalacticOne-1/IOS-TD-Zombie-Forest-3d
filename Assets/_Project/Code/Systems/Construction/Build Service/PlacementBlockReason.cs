namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Причины блокировки строительства.
    /// </summary>
    public enum PlacementBlockReason
    {
        None,
        OutOfGrid,
        CellOccupied,
        WrongFloor,
        TerrainBlocked
    }
}
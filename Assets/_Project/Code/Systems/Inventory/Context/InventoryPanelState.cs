namespace Galactic1.Code.UI.Inventory
{
    public enum InventoryPanelState
    {
        CampFull,          // все режимы, все кнопки
        CampLimited,       // часть кнопок скрыта
        RaidLocked,        // ❗ рейд: только отряд, без управления режимами
        RaidReportLoot,
        RaidReportDrone,
    }
}
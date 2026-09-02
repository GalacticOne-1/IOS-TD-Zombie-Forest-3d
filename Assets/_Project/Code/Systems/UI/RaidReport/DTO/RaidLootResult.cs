
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.UI.RaidReport
{
    /// <summary>
    /// Результат предмета лута после рейда.
    /// DTO для передачи данных в UI.
    /// </summary>
    public struct RaidLootResult
    {
        public ItemConfig Item;
        public int Amount;              // оригинал
        public int TotalAmount;         // после применения бонуса
        public int BonusAmount;         // значение от рекламы
        public int Durability;
        public int AmmoInMagazine;
    }
}
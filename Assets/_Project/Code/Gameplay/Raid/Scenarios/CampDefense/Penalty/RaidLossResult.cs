using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.UI.RaidReport
{
    /// <summary>
    /// Результат ресурса, потерянного при поражении Camp Defense.
    /// DTO для передачи данных в UI. Аналог RaidLootResult,
    /// но без полей ad-бонуса — штраф рекламой не увеличивается/не уменьшается.
    /// </summary>
    public struct RaidLossResult
    {
        public ItemConfig Item;
        public int Amount;
    }
}
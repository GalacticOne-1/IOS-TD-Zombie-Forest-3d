
namespace Galactic1.RaidLoot.Enums
{
    /// <summary>
    /// Стадия истощения контейнера.
    /// Каждая стадия меняет бюджет и пул слотов через multiplier.
    /// </summary>
    public enum DepletionStage
    {
        Full    = 0,   // первое открытие  — 100% budget
        Reduced = 1,   // второе открытие  — 50%  budget, исключаются T3
        Scarce  = 2,   // третье открытие  — 20%  budget, только T1 + Junk
        Empty   = 3    // четвёртое+       — 0%   budget, ничего не генерируется
    }
}

using Galactic1.Code.AbstractFactory;
using Galactic1.Code.Gameplay.Combat.Cover;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.UI.Units.Presentation;

namespace Galactic1.Code.Systems.Raid
{
    // ─────────────────────────────────────────────────────────────────
    //  IUnitSceneContext
    //
    //  Общая часть для любого юнита в сцене — игрока и врага.
    //  UnitInstance.UpdateM() обращается только сюда:
    //    UnitAdapter.Runtime.Tick(dt)  →  IUnitSceneContext.RuntimeBase.Tick(dt)
    //
    //  _Entity видит только ISceneEntityRuntime (Id + Dispose).
    //  UnitInstance видит IUnitSceneContext (RuntimeBase + OnDeath).
    // ─────────────────────────────────────────────────────────────────

    public interface IUnitSceneContext : ISceneEntityRuntime
    {
        /// <summary>
        /// Общий runtime-контракт — тикается каждый кадр из UnitInstance.UpdateM().
        /// Для игрока это RaidUnitRuntime, для зомби — ZombieRuntime.
        /// </summary>
        IUnitRuntimeBase RuntimeBase { get; }
        IUnitStatsScene Stats { get; }
        
        // Используется CoverResolver в HitResolver для определения,
        // блокируется ли выстрел укрытием.
        // Default-реализующий тип должен возвращать CoverType.None
        // если юнит не находится в укрытии.
        UnitCoverState Cover { get; }
    }
}
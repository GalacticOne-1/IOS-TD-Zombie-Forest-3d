using System;
namespace Galactic1.Code.Systems.GameLoop.Tactical
{
    /// <summary>
    /// Единая точка определения следующего тактического состояния.
    /// Используется везде, где рейд завершается (debug-кнопки, Exit Zones и т.п.),
    /// чтобы не дублировать переключательную логику.
    /// </summary>
    public static class TacticalTransitionResolver
    {
        public static Type GetNext(ITacticalState current)
        {
            return current switch
            {
                SUB_RaidActiveState => typeof(SUB_RaidCheckObjectivesState),
                SUB_RaidCheckObjectivesState => typeof(SUB_RaidCleanupState),
                SUB_RaidCleanupState => null,
                _ => throw new ArgumentOutOfRangeException(nameof(current), "Unknown tactical state")
            };
        }
    }
}
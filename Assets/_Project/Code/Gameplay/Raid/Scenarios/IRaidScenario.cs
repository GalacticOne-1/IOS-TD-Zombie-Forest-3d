
using Galactic1.Code.Core;
using Galactic1.Code.Systems.Raid.Mission;
using Galactic1.Core.GameSession;
using Galactic1.Game.Meta.Enemy;

namespace Galactic1.Code.Systems.Raid.Scenarios
{
    /// <summary>
    /// Инкапсулирует поведенческие различия между типами рейдов.
    /// Общий тактический пайплайн (Combat, AI LOD, Spawn, Director,
    /// Unit/Transport lifecycle, Loot/Ambient/ExitZones) остаётся
    /// в RaidInProgressState и управляется через Options — сценарий
    /// не создаёт эти системы и не дублирует их.
    /// </summary>
    public interface IRaidScenario
    {
        /// <summary> Какие общие системы пайплайна включены для этого сценария. </summary>
        ScenarioOptions Options { get; }

        EnemyAIProfile AIProfile { get;}

        /// <summary> Squad/Transport/прочие scenario-специфичные данные в RaidRuntime. Вызывается до spawn pipeline. </summary>
        void Configure(RaidRuntime raid);

        /// <summary>
        /// Только для систем, которых в общем пайплайне ещё нет
        /// (например turrets/camp buildings у Camp Defense).
        /// Loot/Ambient/ExitZones сюда не входят — они гейтятся через Options
        /// и создаются самим RaidInProgressState.
        /// </summary>
        void OnSceneLoaded(SceneSessionDefinition scene);

        /// <summary> Тактический бой стартовал (SUB_RaidStartState). </summary>
        void OnBattleStarted();

        /// <summary> Бой завершён (SUB_RaidCleanupState / OnRaidFinished). </summary>
        void OnBattleFinished();

        /// <summary>
        /// Освобождает ТОЛЬКО то, что сценарий сам создал в OnSceneLoaded.
        /// Общие системы освобождает RaidInProgressState.Exit() по тем же Options.
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Применение результата в прокси объектов
        /// </summary>
        void ApplyResults();

        /// <summary>
        /// Выход из локации согласно сценарию
        /// </summary>
        void ExitFromLocation();
        
        /// <summary>
        /// Все боевые силы игрока уничтожены.
        /// </summary>
        bool ArePlayerForcesDestroyed(MissionStateProvider state);
        
        /// <summary>
        /// Чистая функция: оценивает состояние миссии по снимку MissionContext.
        /// Никогда не резолвит сервисы, не хранит ссылки на WaveSpawner/HQRuntime и т.п. —
        /// весь необходимый контекст уже находится в переданном MissionContext.
        /// </summary>
        MissionResult EvaluateMission(MissionContext context);

        
        /// <summary>
        /// Рассчитывает финальный результат рейда.
        /// Вызывается ОДИН раз при завершении тактического слоя.
        /// </summary>
        RaidResultProxy BuildRaidResult(RaidRuntime raid, MissionResult mission);

    }
}
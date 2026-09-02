
using Galactic1.Code.Systems.Raid.Enemies;

namespace Galactic1.Code.Gameplay.Enemies.Spawning
{
    /// <summary>
    /// Иммутабельный результат операции спавна одного врага.
    ///
    /// Возвращается из EnemySpawnPipeline.Spawn().
    /// Используется для:
    ///   — дебага и логирования
    ///   — статистики волн (WaveSystem)
    ///   — хуков Director AI
    ///   — тестов
    ///
    /// Если Success == false — Runtime == null, FailureReason содержит причину.
    /// </summary>
    public readonly struct EnemySpawnResult
    {
        /// <summary>true если спавн завершился успешно.</summary>
        public readonly bool Success;

        /// <summary>Созданный runtime. null если Success == false.</summary>
        public readonly EnemyRuntime Runtime;

        /// <summary>Причина неудачи. Пустая строка если Success == true.</summary>
        public readonly string FailureReason;

        private EnemySpawnResult(bool success, EnemyRuntime runtime, string failureReason)
        {
            Success = success;
            Runtime = runtime;
            FailureReason = failureReason;
        }

        /// <summary>Конструктор успешного результата.</summary>
        public static EnemySpawnResult Succeeded(EnemyRuntime runtime) =>
            new(true, runtime, string.Empty);

        /// <summary>Конструктор результата с ошибкой.</summary>
        public static EnemySpawnResult Failed(string reason) =>
            new(false, null, reason);

        public override string ToString() =>
            Success
                ? $"[SpawnResult] OK | Id={Runtime?.Id}"
                : $"[SpawnResult] FAIL | {FailureReason}";
    }
}
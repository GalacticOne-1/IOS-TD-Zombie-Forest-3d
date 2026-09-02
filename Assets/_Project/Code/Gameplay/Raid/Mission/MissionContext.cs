using System.Collections.Generic;

namespace Galactic1.Code.Systems.Raid.Mission
{
    /// <summary>
    /// Агрегированное состояние миссии — единственный вход для Scenario.EvaluateMission().
    /// Обновляется исключительно MissionObjectiveService в ответ на игровые события.
    ///
    /// Сценарий никогда не резолвит WaveSpawner/HQRuntime/ExitZoneManager напрямую —
    /// он читает только эти сигналы. Это делает EvaluateMission чистой функцией:
    /// (MissionContext) => MissionResult, без побочных обращений к DI/ServiceLocator.
    ///
    /// Общие для большинства combat-сценариев сигналы — явные поля.
    /// Специфичные для будущих сценариев (Escort/Boss/Timed Survival) сигналы —
    /// через Flags/Counters, чтобы не переписывать этот класс при каждом новом сценарии.
    /// </summary>
    public class MissionContext
    {
        // ── Общие сигналы ──────────────────────────────────────────────────
        public bool PlayerForcesDestroyed;

        // ── Именованные сигналы существующих сценариев ─────────────────────
        public bool ExitReached; // Exploration
        public bool AllWavesCompleted; // CampDefense
        public bool HeadquartersDestroyed; // CampDefense

        // ── Расширяемый набор для будущих сценариев ────────────────────────
        // Например: Escort → Flags["EscortTargetLost"]
        //           Boss → Flags["BossDefeated"]
        //           Timed Survival → Counters["SecondsRemaining"]
        private readonly Dictionary<string, bool> _flags = new();
        private readonly Dictionary<string, int> _counters = new();

        public bool GetFlag(string key) => _flags.TryGetValue(key, out var v) && v;
        public void SetFlag(string key, bool value) => _flags[key] = value;

        public int GetCounter(string key) => _counters.TryGetValue(key, out var v) ? v : 0;
        public void SetCounter(string key, int value) => _counters[key] = value;
    }
}
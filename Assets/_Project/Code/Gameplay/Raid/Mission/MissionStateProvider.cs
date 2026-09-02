using System.Linq;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime.Building;

namespace Galactic1.Code.Systems.Raid.Mission
{
    /// <summary>
    /// Единая точка получения текущего состояния миссии.
    ///
    /// Не принимает решений о победе/поражении.
    /// Не знает про сценарии.
    ///
    /// Его единственная задача —
    /// агрегировать состояние игрового мира
    /// (отряд, защитники лагеря, HQ, волны и т.д.).
    /// </summary>
    public class MissionStateProvider
    {
        private readonly RaidRuntime _raid;
        private readonly GameLoopContext _context;

        public MissionStateProvider(
            RaidRuntime raid,
            GameLoopContext context)
        {
            _raid = raid;
            _context = context;
        }

        /// <summary>
        /// Проверка только тактического отряда.
        /// </summary>
        public bool IsSquadDestroyed()
        {
            return !_raid.Squad.HasAliveUnits;
        }

        /// <summary>
        /// Проверка всех защитников лагеря.
        /// </summary>
        public bool AreCampDefendersDestroyed()
        {
            bool squadDead = !_raid.Squad.HasAliveUnits;
            bool campDead = !_context.CampUnits.Any(u => !u.Stats.IsDead);

            return squadDead && campDead;
        }

        /// <summary>
        /// Главный штаб уничтожен.
        /// Пока заглушка.
        /// </summary>
        /// <summary>
        /// Главный штаб уничтожен.
        /// </summary>
        public bool IsHeadquartersDestroyed()
        {
            var mainBase = _context.CurrentRaid.DefenseFacilities
                .GetFacility(FacilityType.CampHQ);
            return mainBase != null && mainBase.IsDestroyed;
        }

        /// <summary>
        /// Все волны завершены.
        /// Пока заглушка.
        /// </summary>
        public bool AreAllWavesCompleted()
        {
            return _raid.WaveProgress?.IsDefenseCompleted ?? false;
        }
    }
}
using System.Linq;
using Galactic1.Code.Systems.GameLoop;

namespace Galactic1.Code.Systems.Squad
{
    public enum SquadValidationResult
    {
        Success,
        EmptySquad,
        NoUnitsInCamp,
    }

    /// <summary>
    /// Единственная точка входа для проверки готовности отряда
    /// перед World Map и Camp Defense.
    /// Не запускает сцены, не показывает UI, не решает штрафы.
    /// </summary>
    public sealed class SquadValidationService : IGameService
    {
        private readonly GameLoopContext _context;

        public SquadValidationService(GameLoopContext context)
        {
            _context = context;
        }

        public SquadValidationResult ValidateForWorldMap()
        {
            return _context.StrategicSquadUnits.Any()
                ? SquadValidationResult.Success
                : SquadValidationResult.EmptySquad;
        }

        public SquadValidationResult ValidateForCampDefense()
        {
            if (_context.StrategicSquadUnits.Any())
                return SquadValidationResult.Success;

            return _context.PlayerUnits.Any()
                ? SquadValidationResult.EmptySquad
                : SquadValidationResult.NoUnitsInCamp;
        }
    }
}
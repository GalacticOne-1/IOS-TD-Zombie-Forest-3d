
using Galactic1.Code.Gameplay.Units;

namespace Galactic1.Code.Gameplay.Damage
{
    public static class TeamService
    {
        public static bool CanDamage(IUnitRuntimeBase a, IUnitRuntimeBase b)
        {
            if (a == null || b == null) return true; // окружение
            return a.TeamId != b.TeamId;
        }
    }
}
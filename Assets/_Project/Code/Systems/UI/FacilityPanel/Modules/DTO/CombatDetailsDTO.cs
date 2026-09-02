using Galactic1.Code.Systems.Runtime.Building;

namespace Galactic1.Game.UI.Buildings.DTO
{
    sealed class CombatDetailsDTO : IFacilityDetailsDTO
    {
        public FacilityType Type => FacilityType.Defense;

        public int HP { get; }
        public float Damage { get; }


        public CombatDetailsDTO(
            int hp, 
            float damage)
        {
            HP = hp;
            Damage = damage;
        }
    }
}
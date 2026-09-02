namespace Galactic1.Code.Gameplay.Damage
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "TargetPriorityProfile", menuName = "Game Configs/AI/Target Priority Profile")]
    public class TargetPriorityProfile : ScriptableObject
    {
        [Header("Базовые веса приоритета")]
        public int BasePriority = 0;

        [Header("Вес факторов")]
        public float DistanceWeight = -1f; // чем дальше, тем ниже приоритет
        public float HealthWeight = -0.5f; // чем меньше HP, тем ниже приоритет (или выше, если хотите добивать)
        public float RoleWeightMultiplier = 1f; // множитель для роли (босс, элита)

        [Header("Бонусы")]
        public int BossBonus = 10;
        public int EliteBonus = 5;
        public int CarryingBombBonus = 8;

        [Header("Нормализация")]
        public float MaxDistance = 20f; // для нормализации расстояния
    }

}
using UnityEngine;

namespace Galactic1.Code.WorldMap
{
    /// <summary>
    /// Конфигурация стоимости посещения локации
    /// </summary>
    [CreateAssetMenu(fileName = "LocationVisitCostConfig", menuName = "Game Configs/World Map/Location Visit Cost Config")]
    public class LocationVisitCostConfig : ScriptableObject
    {
        [Header("Base Cost")] [SerializeField] 
        private float BaseDays = 1f;

        [Header("Difficulty Multiplier")] [SerializeField]
        private AnimationCurve DifficultyMultiplier;

        [Header("Camp Modifiers")] [SerializeField]
        private float CampModifier = 0.7f;

        
        public float GetCost(int difficulty, bool isCamp)
        {
            float cost = BaseDays;

            if (DifficultyMultiplier != null)
                cost *= DifficultyMultiplier.Evaluate(difficulty);

            if (isCamp)
                cost *= CampModifier;

            return cost;
        }
    }
}
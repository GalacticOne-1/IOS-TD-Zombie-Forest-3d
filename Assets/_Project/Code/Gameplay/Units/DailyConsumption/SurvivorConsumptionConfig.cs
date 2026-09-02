using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Units.Stats;
using UnityEngine;

namespace Galactic1.Meta.Configs.Recruitment
{
    [CreateAssetMenu(
        fileName = "SurvivorConsumptionConfig",
        menuName = "Game Configs/Player/Survivor Consumption Config")]
    public class SurvivorConsumptionConfig : ScriptableObject
    {
        public RuntimeId FoodItemId;
        public RuntimeId WaterItemId;

        public Buff HungerBuff;
        public Buff ThirstBuff;
    }
}
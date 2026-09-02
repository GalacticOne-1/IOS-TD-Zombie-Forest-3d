using System;
using Galactic1.Configs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Code.Systems.Economy.Configs
{
    [CreateAssetMenu(
        fileName = "EconomyConfig", 
        menuName = "Game Configs/Economics/Economy Config")]
    public class EconomyConfig : ScriptableObject, IUpdateFromJson
    {
        [field: SerializeField] public int ProductionCostPerHour { get; private set; }
       
        [field: Header("=== Tavern")]
        [field: SerializeField] public int RefreshPremium { get; private set; }
        [field: SerializeField] public int RecruitPremium { get; private set; }
        
        [field: Header("=== Cargo Drone")]
        [field: SerializeField] public int CargoDroneCostPremium { get; private set; }
        [field: SerializeField] public int CargoDroneMaxCharge { get; private set; }
        
        
        [Serializable]
        private class Wrapper
        {
            public EconomyDataWrapper economyData;
        }

        
        
        
        
        /// <summary>
        /// Обновить поля ScriptableObject из JSON.
        /// </summary>
        public void UpdateFromJson(string json)
        {
            try
            {
                var wrapper = JsonUtility.FromJson<Wrapper>(json);
                if (wrapper != null && wrapper.economyData != null)
                {
                    ProductionCostPerHour = wrapper.economyData.production_cost_per_hour;
                    CargoDroneCostPremium = wrapper.economyData.cargo_drone_cost_premium;
                    CargoDroneMaxCharge = wrapper.economyData.cargo_drone_max_charge;
                    RecruitPremium = wrapper.economyData.tavern.recruit_premium;
                    RefreshPremium = wrapper.economyData.tavern.refresh_premium;
                }
                else
                {
                    Debug.LogWarning("⚠️ UpdateFromJson: JSON не содержит данных EconomyData.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ UpdateFromJson error: {e.Message}");
            }
        }
    }

    [Serializable]
    public class EconomyDataWrapper
    {
        public int production_cost_per_hour;
        public int cargo_drone_cost_premium;
        public int cargo_drone_max_charge;
        public EconomyTavern tavern;
    }


    [Serializable]
    public struct EconomyTavern
    {
        public int refresh_premium;
        public int recruit_premium;
    }
}
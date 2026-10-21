using System;
using UnityEngine;

namespace Galactic1.Code.Systems.Economy.Configs
{
    [CreateAssetMenu(
        fileName = "EconomyConfig", 
        menuName = "Game Configs/Economics/Economy Config")]
    public class EconomyConfig : ScriptableObject
    {
        [field: SerializeField] public int ProductionCostPerHour { get; private set; }
       
        [field: Header("=== Tavern")]
        [field: SerializeField] public int RefreshPremium { get; private set; }
        [field: SerializeField] public int RecruitPremium { get; private set; }
        
        [field: Header("=== Cargo Drone")]
        [field: SerializeField] public int CargoDroneCostPremium { get; private set; }
        [field: SerializeField] public int CargoDroneMaxCharge { get; private set; }
        
    }
}
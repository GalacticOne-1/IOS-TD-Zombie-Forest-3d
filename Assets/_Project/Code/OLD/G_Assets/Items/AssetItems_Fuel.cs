using UnityEngine;

namespace Galactic1
{
    [CreateAssetMenu(fileName = "Goods Fuel", menuName = "Assets/Player Units/Goods Fuel", order = 0)]
    public class AssetItems_Fuel : AssetItems
    {

        
        [Header("Время работы 1ед топлива")]
        [SerializeField] private int fuelDuration;

        public int FuelDuration => fuelDuration;
        
        
    }
}
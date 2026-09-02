
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    
    [CreateAssetMenu(fileName = "Equipment", menuName = "Assets/Player Units/Equipment", order = 0)]
    public class AssetInventory_equipment : AssetEquipmement, IItemUsing
    {

        public override string GetMainFeatures()
        {
            GetAttribute(StatId.Armor, out CAttributes attr);
            return $"Armor: {attr.value}";
        }
    }
}
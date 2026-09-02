
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    
    [CreateAssetMenu(fileName = "Weapon", menuName = "Assets/Player Units/Weapon", order = 0)]
    public class AssetInventory_weapon : AssetEquipmement, IItemUsing
    {

        [Space] 
        public EEquipmentType equipmentType;
        
        [SerializeField]
        protected Galactic1.CWeaponSetup weaponDefault;


        #region PRIVATE


        public Galactic1.CWeaponSetup wp => weaponDefault;

        

        #endregion
        
        
        public override string GetMainFeatures()
        {
            return $"Damage: {weaponDefault.damage} \nAttack Speed: {Mathf.CeilToInt(60 / weaponDefault.reload)}";
        }
        


        protected override void InitItem(GameObject module)
        {
            //module.GetComponent<WeaponABS>().weapon = weaponDefault;
        }
        
    }
}
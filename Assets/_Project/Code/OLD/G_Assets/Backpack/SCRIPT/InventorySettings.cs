
using Galactic1;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1
{
    public abstract class InventoryConfigs : _AttributesConfigs_, IAssetHeader, IAssetSorting, IAssetHint
    {

        [Space]
        public EEquipmentFiltr filtr;
        public EInventorySlot[] availableSlot;

        [Space] 
        [SerializeField] 
        private bool stackable;
        public byte levelRequired;
        public ERarities rare;
        [SerializeField] 
        private int sortingTypeOrder, sortingOrder;
       


        
        public bool Stackable => stackable;
        public ERarities Rare => rare;

        [Header("Приоритет сортировки")]
        [SerializeField] private ESorting sorting;
        public ESorting Sorting => sorting;
        
        public int SortingTypeOrder => sortingTypeOrder;
        public int SortingOrder => sortingOrder;


        
        [Header("Требования для крафта (разные варианты)")]
        public short timeProduction;
        public bool requireFuel;
        [SerializeField]
        private CCraft requireItem1;
        [SerializeField]
        private CCraft requireItem2;
        [SerializeField]
        private CCraft requireItem3;
        [SerializeField]
        private CCraft requireItem4;

        public CCraft Recipe1 => requireItem1;
        public CCraft Recipe2 => requireItem2;
        public CCraft Recipe3 => requireItem3;
        public CCraft Recipe4 => requireItem4;


        [System.Serializable]
        public struct CCraft
        {
            public byte craftedOutput;
            
            
            public CCraft1[] inputItem;
        }
        
        [System.Serializable]
        public struct CCraft1
        {
            public short amount;
            public EItems item;
            
            [FormerlySerializedAs("useEquipmentToCraft")] [Header("true - брать предмет из снаряжения")]
            public bool useEquipment;
            public EEquipment equipment;
        }


        public abstract int GetAssetKey();
        
        public abstract string GetMainFeatures();
    }


    public enum EEquipmentFiltr
    {
        WORKBENCH, DEFENSE,  TOOL, WEAPON, ARMOR,
    }


    // для определения используемых предметов через инвентарь
    public interface IItemUsing{}
}
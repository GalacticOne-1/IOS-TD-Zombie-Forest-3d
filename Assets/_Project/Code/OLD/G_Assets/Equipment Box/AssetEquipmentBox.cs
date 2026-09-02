using Galactic1;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1
{
    [CreateAssetMenu(fileName = "Equipment Box", menuName = "Assets/Equipment Box", order = 0)]
    public class AssetEquipmentBox : ScriptableObject
    {

        [SerializeField] private CDataBox[] standart;
        public CDataBox[] Standart => standart;
        
        
        [SerializeField] private CDataBox[] superior;

        public CDataBox[] Superior => superior;
        
        


        [System.Serializable]
        public struct CDataBox
        {
            public bool goods;
            public EItems assetKey1;
            public EEquipment assetKey2;
            public byte volume;
        }


    }
}
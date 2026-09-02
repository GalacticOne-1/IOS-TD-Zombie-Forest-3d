using Galactic1;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1
{
    public abstract class AssetEquipmement : InventoryConfigs
    {

        [FormerlySerializedAs("typeItem")]
        [Space] 
        [SerializeField] private EEquipment assetKey;
        public override int GetAssetKey() => (int)assetKey;
        
        
        [SerializeField] private short durability;
        public short Durability => durability;
        

        [Space] 
        [SerializeField] private CSkin skinSprite;
        public CSkin SkinSprite => skinSprite;

        [System.Serializable]
        public struct CSkin
        {
            public Sprite main;
            public Sprite[] arms, legs;
        }
        
        
        
        
        
        
        public GameObject CreateItem(Transform hold)
        {
            GameObject g = null;//prefab.CreateGO(hold);
            InitItem(g);
            return g;
        }

        protected virtual void InitItem(GameObject module) {}
    }
}
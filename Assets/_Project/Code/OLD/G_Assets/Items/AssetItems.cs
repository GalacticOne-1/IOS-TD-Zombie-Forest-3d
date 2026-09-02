using Galactic1;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1
{
    [CreateAssetMenu(fileName = "Goods", menuName = "Assets/Player Units/Goods", order = 0)]
    public class AssetItems : InventoryConfigs
    {
        
        [Header("Что за предмет")]
        public EItems assetKey;

        [Header("Остаток после использования")]
        public bool outUse;
        [FormerlySerializedAs("outAfterUse")] public EItems outUseAsset;
        
        [SerializeField] private CUsing[] useResult;
        public CUsing[] UseResult => useResult;

        [System.Serializable] 
        public struct CUsing
        {
            public EUsing result;
            public sbyte volume;
        }
        

        
        public override int GetAssetKey() => (int)assetKey;

        public override string GetMainFeatures() => "";
    }
}
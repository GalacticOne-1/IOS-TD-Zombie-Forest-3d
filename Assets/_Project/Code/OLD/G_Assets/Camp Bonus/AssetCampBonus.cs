
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    [CreateAssetMenu(fileName = "Camp Bonus", menuName = "Assets/Daily Reward/Camp Bonus")]
    public class AssetCampBonus : ScriptableObject, INewSaveData
    {

        [SerializeField] private CData[] list;

        public CData[] List => list;


        [System.Serializable] 
        public struct CData
        {
            public EEquipment equipment;
            public byte adQu;
            public byte cost;
        }

        
        
        
        
        
        
        
        public void NewSaveData()
        {
            // GAMEPLAY_old.DataGameplay().campBonus = new CSaveCampBonus[list.Length];
            //
            // var l = list.Length;
            // for (int i = 0; i < l; i++)
            // {
            //     GAMEPLAY_old.DataGameplay().campBonus[i] = new CSaveCampBonus();
            // }
        }
        
    }
}
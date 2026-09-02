using UnityEngine;

namespace Galactic1.Code.Dev
{
    public enum EStartKit
    {
        StartGame_01 = 10,
        StartGame_02 = 11,
        StartGame_03 = 12,
            
            
        PurchaseReward = 50,
            
            
        AllResources = 60,
        ConstructionKit = 61,
            
        AllWeapons = 100,
        AllArmors = 101,
        AllAmmo = 102,
    }
    
    [CreateAssetMenu(fileName = "StartKitData", menuName = "Game Configs/Start World/Start Kit Data")]
    public class StartKitData : ScriptableObject
    {
        [SerializeField] private StartKit[] kit;


        [System.Serializable] 
        public struct StartKit
        {
            public EStartKit type;
            public StartKitConfigBase config;
        }
        


        public StartKitConfigBase GetKit(EStartKit type)
        {
            var l = kit.Length;
            for (int i = 0; i < l; i++)
            {
                if(type == kit[i].type)
                    return kit[i].config;
            }

            Debug.LogError($"Starter Kit not exist >> {type}");
            return kit[0].config;
        }
    }
}
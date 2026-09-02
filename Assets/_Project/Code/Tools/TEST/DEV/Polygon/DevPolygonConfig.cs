using Galactic1.AbstractFactory;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace DEV
{
    [CreateAssetMenu(fileName = "DevPolygonConfig", menuName = "Game Configs/Dev/Dev Polygon Config")]
    public class DevPolygonConfig : ScriptableObject
    {
        public Vector3 startCoord;
        

        public CData[] devList;
        
        [Space]
        public bool realEnemy;
        public CDataReal[] realList;
        
        
        [System.Serializable]
        public struct CData
        {
            public _Entity prefab;
        }
        [System.Serializable]
        public struct CDataReal
        {
            public EnemyId configId;
        }
    }
}
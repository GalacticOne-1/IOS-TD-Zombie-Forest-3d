
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Window
{
    public class WindowCardInitialStateConfigs : ScriptableObject
    {
        [field: SerializeField] public bool Use { get; private set; }
        
        [field: Space(10)]
        [field: SerializeField] public EWindowCardType CardType { get; private set; }
        [field: SerializeField] public IAPId Id { get; private set; }
       
        [field: SerializeField] public string PrefabPath { get; private set; }
        [field: SerializeField] public int CardVariant { get; private set; }
        
        
        
        [field: Space(10)]
        #region HEADER

        [field: SerializeField] public CHeader Header { get; private set; }
        
        [System.Serializable] 
        public struct CHeader
        {
            public string TitleLid;
            [TextArea]
            public string DescriptionLid;

            public int Order;
            
            [Space]
            public Sprite Icon;
            public float SizeUI;
            public Vector2 IconOffset;
        }
        
        #endregion
    }
}
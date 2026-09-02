using UnityEngine;

namespace Galactic1.Game.Meta.Stats
{
    // Единый реестр всех статов в игре.
    
    [CreateAssetMenu(fileName = "StatDefinition", menuName = "Game Configs/Inventory/Stat Definition")]
    public class StatDefinition : ScriptableObject
    {
        public StatId id;
        public float defaultValue;
        public bool isInteger;
    }
}
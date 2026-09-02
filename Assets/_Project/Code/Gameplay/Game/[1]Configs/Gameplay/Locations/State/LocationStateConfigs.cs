using UnityEngine;

namespace Galactic1.Configs
{
    [CreateAssetMenu(fileName = "LocationStateConfigs", menuName = "Game Configs/Maps/New Location State Configs", order = 0)]
    public class LocationStateConfigs : ScriptableObject
    {
        [field: SerializeField] public string ConfigId { get; private set; }
        public string Id
        {
            get => ConfigId;
            set => ConfigId = value;
        }
        
        public int LocationId;
        public LocationInitialStateConfigs initialStateConfigs;
    }
}
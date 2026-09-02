using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Interaction;
using UnityEngine;

namespace Galactic1.Code.WorldMap
{
    /// <summary>
    /// Узел карты — локация, куда игрок может ехать.
    /// </summary>
    public class MapNode : MonoBehaviour, IInteractable
    {
        [field: SerializeField] public LocationId Id { get; private set; }

        [field: SerializeField] public bool IsCamp { get; private set; }
        
        [field: Header("=== STATE ===")]
        [field: SerializeField] public bool IsCleared { get; private set; }
        [field: SerializeField] public bool IsDiscovered { get; private set; }
        [field: SerializeField] public bool IsAvailable { get; private set; }
        
        
        
        public LocationConfig Config { get; private set; }
        public LocationConfig SetConfig { set => Config = value; }
        public GameObject GetObject => gameObject;
        
        

        /// <summary>
        /// target, visitCost, daysUntilThreat
        /// </summary>
        public Action<MapNode> OnNodeClicked;
        public event Action<MapNode> OnNodeStateChanged;
        
        
        public void OnInteract()
        {
            OnNodeClicked?.Invoke(this);
        }
        
        public void SetDiscovered(bool discovered)
        {
            IsDiscovered = discovered;
            OnNodeStateChanged?.Invoke(this);
        }
        
        public float GetVisitCost()
        {
            if (Config.VisitCostConfig == null)
            {
                Debug.LogError($"VisitCostConfig missing on {name}");
                return 0f;
            }

            return Config.VisitCostConfig.GetCost(Config.Difficulty, IsCamp);
        }
    }
}
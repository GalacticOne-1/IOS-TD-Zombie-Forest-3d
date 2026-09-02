using UnityEngine;

namespace Galactic1.Core.Systems.GameSession.WorldMap
{
    [CreateAssetMenu(fileName = "WorldMapConfig", menuName = "Game Configs/World Map/World Map Config")]
    public class WorldMapConfig : ScriptableObject
    {
        [field: Header("=== CAMERA ===")]
        
        [field: SerializeField] public Vector3 CameraPosition { get; private set; }
        [field: SerializeField] public Vector3 CameraMinBounds { get; private set; }
        [field: SerializeField] public Vector3 CameraMaxBounds { get; private set; }
        [field: SerializeField] public float CameraZoomMin { get; private set; } = 5f;
        [field: SerializeField] public float CameraZoomMax { get; private set; } = 20f;
        
        
    }
}
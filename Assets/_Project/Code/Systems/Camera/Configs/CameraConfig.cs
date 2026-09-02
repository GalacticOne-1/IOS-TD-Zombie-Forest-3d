using UnityEngine;

namespace Galactic1.Code.Cameras.Configs
{
    /// <summary>
    /// Camera configuration asset.
    /// Contains all tunable parameters that define camera behavior.
    /// Does NOT contain runtime state or level-specific data.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "Game Configs/Camera/Camera Config")]
    public class CameraConfig : ScriptableObject
    {
        [field: Header("Squad Radius Limit")]
        [field: SerializeField] public bool LimitToSquadRadius { get; private set; } = false;

        [field: Min(0.1f)]
        [field: SerializeField] public float SquadRadiusLimit { get; private set; } = 15f;
        
        [field: Header("Movement")]
        [field: SerializeField] public bool UseCameraBounds { get; private set; } = false;
        [field: Min(0.01f)]
        [field: SerializeField]
        public float MoveSpeed { get; private set; } = 10;
        
        [field: Min(0.001f)]
        [field: SerializeField]
        public float DragSpeed { get; private set; } = 0.2f;
        [field: SerializeField] public float DragResponsiveness { get; private set; } = 18f; 

        
        [field: Header("Focus")]
        [field: SerializeField] public float FocusOffsetZ { get; private set; }
        [field: SerializeField] public float FocusDuration { get; private set; } = .2f;
        [field: SerializeField] public float FacilityFocusViewportOffsetY { get; private set; } = 0.15f;
        [field: SerializeField] public float ConstructionModeFocusViewportOffsetY { get; private set; } = -0.15f;
        
        [field: Header("Inertia")]
        [field: Min(0f)]
        [field: SerializeField]
        public float InertiaDamping { get; private set; } = 8f;

        [field: Min(0f)]
        [field: SerializeField]
        public float MinInertiaSpeed { get; private set; } = 0.05f;

        [field: Header("Zoom")]
        [field: Min(0.01f)]
        [field: SerializeField]
        public float ZoomSpeed { get; private set; } = 5f;
        
        [field: SerializeField] 
        public float ZoomSmoothTime { get; private set; }  = 0.15f;
        [field: SerializeField] 
        public float PinchZoomSpeed { get; private set; } = 0.02f;

        [field: Min(0.1f)]
        [field: SerializeField]
        public float MinZoom { get; private set; } = 3f;

        [field: Min(0.1f)]
        [field: SerializeField]
        public float MaxZoom { get; private set; } = 10f;
        
        [field: SerializeField] 
        public float ConstructionMaxZoom { get; private set; } = 20f;
        
        [field: Header("Tilt")]
        [field: SerializeField] 
        public float DefaultTilt { get; private set; } = 45f;
        [field: SerializeField] 
        public float ConstructionTilt { get; private set; } = 60f;
        [field: SerializeField] 
        public float TiltSmooth { get; private set; } = 0.25f;

        
        [field: Header("Input")]
        [field: SerializeField]
        public KeyCode DragButton { get; private set; } = KeyCode.Mouse0;

        [field: SerializeField] public bool InvertDrag { get; private set; } = false;
        
    }
}

using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Code.Cameras.Configs
{
    [CreateAssetMenu(fileName = "CameraConfigs", menuName = "Game Configs/Camera/Camera Configs")]
    public class CameraConfigs : ScriptableObject
    {
        [SerializeField] private CameraConfig defaultCamera;
        [SerializeField] private CameraConfig campCamera;
        [SerializeField] private CameraConfig locationCamera;
        [SerializeField] private CameraConfig worldmapCamera;


        public CameraConfig DefaultCamera => defaultCamera;

        public CameraConfig CampCamera => campCamera;

        public CameraConfig LocationCamera => locationCamera;

        public CameraConfig WorldmapCamera => worldmapCamera;
    }
}
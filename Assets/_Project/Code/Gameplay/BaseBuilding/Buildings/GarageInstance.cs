using UnityEngine;

namespace Galactic1.Code.Gameplay.BaseBuilding
{
    public class GarageInstance : CampFacilityInstance
    {
        [Header("Spawn Point")]
        [Tooltip("Точка спавна транспорта около гаража")]
        [SerializeField] private Transform transportSpawnPoint;

        public Transform TransportSpawnPoint => transportSpawnPoint;
    }
}
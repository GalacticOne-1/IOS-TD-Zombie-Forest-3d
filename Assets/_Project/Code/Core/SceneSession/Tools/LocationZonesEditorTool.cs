using UnityEngine;

namespace Galactic1.Gameplay.Locations.Authoring
{
    /// <summary>
    /// Вешается на префаб локации. Хранит только данные (ссылки на префабы и родителя для zone_root),
    /// вся логика создания/удаления построена на иерархии Transform'ов и обслуживается
    /// LocationZonesEditorToolEditor (Editor-only).
    /// </summary>
    public class LocationZonesEditorTool : MonoBehaviour
    {
        [SerializeField] private Transform zoneRootParent;
        [SerializeField] private GameObject zonePrefab;
        [SerializeField] private GameObject lootPrefab;
        [SerializeField] private GameObject zombiePrefab;

        public Transform ZoneRootParent => zoneRootParent != null ? zoneRootParent : transform;
        public GameObject ZonePrefab => zonePrefab;
        public GameObject LootPrefab => lootPrefab;
        public GameObject ZombiePrefab => zombiePrefab;

        public const string ZonePrefix = "zone_";
        public const string LootSpawnName = "loot_spawn";
        public const string ZombieSpawnName = "zombie_spawn";
    }
}
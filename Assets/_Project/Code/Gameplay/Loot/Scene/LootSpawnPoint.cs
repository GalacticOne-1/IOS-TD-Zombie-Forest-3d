
using Galactic1.RaidLoot.Authoring;
using UnityEngine;

namespace Galactic1.RaidLoot.Scene
{
    /// <summary>
    /// Единственный Scene Authoring объект лутового контейнера.
    /// Хранит конфиг и позицию. LootContainerFactory читает его при старте рейда.
    /// Заменяет LootContainerAuthoring — дублирования нет.
    /// </summary>
    public sealed class LootSpawnPoint : MonoBehaviour
    {
        [SerializeField] private LootContainerDefinitionConfig _config;


        public LootContainerDefinitionConfig Config => _config;
        public LootContainerView View => GetComponent<LootContainerView>();
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_config == null) return;
            
            // граница для лута, то что будет закрыто для сетки
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1.5f);
            
            // просто закрашиваем реально закрытую для навигации область
            Gizmos.color = Color.black;
            Gizmos.DrawCube(transform.position, new Vector3(4,.6f,4));
            
            Gizmos.color = new Color(.16f, 0.35f, 1f, 0.25f);
            Gizmos.DrawSphere(transform.position, _config.OpenRadius);
            Gizmos.color = new Color(.16f, 0.35f, 1f, 1f);
            Gizmos.DrawWireSphere(transform.position, _config.OpenRadius);
        }
#endif
    }
}
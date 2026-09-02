
using Galactic1.Configs.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Authoring
{
    /// <summary>
    /// Точка группового спавна врагов.
    /// Размещается на сцене и является единственным источником
    /// пространственных данных для группы.
    ///
    /// Позиция = transform.position.
    /// Одна и та же AmbientGroupConfig может использоваться
    /// на разных сценах с разными радиусами.
    /// </summary>
    public sealed class EnemySpawnPoint : MonoBehaviour
    {
        [Tooltip("Конфиг состава группы: кто спавнится.")]
        public EnemyGroupConfig Group;

        [Tooltip("Радиус блуждания группы вокруг этой точки.")]
        public float WanderRadius = 10f;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.25f);
            Gizmos.DrawSphere(transform.position, WanderRadius);
            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, WanderRadius);
        }
#endif
    }
}
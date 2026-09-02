using Galactic1.Code.Systems.Raid;
using Galactic1.Gameplay.Locations.Definitions;
using Galactic1.Gameplay.Locations.Events;
using UnityEngine;

namespace Galactic1.Gameplay.Locations.Authoring
{
    /// <summary>
    /// Триггер-зона выхода из локации (эвакуация / завершение миссии).
    /// НЕ содержит логики завершения рейда — только детект коллизии
    /// и передача события через EventBus.
    ///
    /// Детект отряда — через LayerMask, без тегов и FindObjectOfType,
    /// чтобы не зависеть от конкретной реализации юнита игрока.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class SceneExitZone : MonoBehaviour
    {
        [Header("=== IDENTITY ===")] [SerializeField]
        private ExitId exitId;

        [Header("=== RESULT ===")]
        [Tooltip("Статус рейда, который будет установлен при входе в зону.")]
        [SerializeField]
        private RaidStatus resultStatus = RaidStatus.Completed;

        [Tooltip("Причина завершения рейда.")] [SerializeField]
        private RaidEndReason resultReason = RaidEndReason.ObjectivesCompleted;

        [Tooltip("Множитель размера визуала относительно коллайдера.")] [SerializeField]
        private float visualScaleMultiplier = 1f;

        private const string PlayerTag = "Player";

        // Защита от повторного срабатывания несколькими коллайдерами отряда
        // (несколько survivor'ов могут войти в зону почти одновременно).
        private bool _triggered;

        private void Awake()
        {
            var col = GetComponent<BoxCollider>();
            if (!col.isTrigger)
                Debug.LogWarning($"[SceneExitZone] Collider on '{name}' is not marked as Trigger.");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered || !other.CompareTag(PlayerTag))
                return;
            
            _triggered = true;
            
            // очищаем сразу после вызова, т.к выходим из сцены
            EventBus<ExitZoneTriggerEvent>.Raise(new ExitZoneTriggerEvent(exitId, resultStatus, resultReason));
        }

#if UNITY_EDITOR

        /// <summary>
        /// Синхронизирует масштаб visualChild с размером коллайдера +5%.
        /// Выполняется только в редакторе, при изменении полей в инспекторе.
        /// Не является игровой логикой — чистое авторинг-удобство.
        /// </summary>
        private void OnValidate()
        {
            SyncVisualScale();
        }

        private void SyncVisualScale()
        {
            var visualBorder = transform.GetChild(0);
            if (visualBorder == null)
                return;

            var col = GetComponent<Collider>();
            if (col == null)
                return;

            Vector3 size = GetColliderLocalSize(col);
            if (size == Vector3.zero)
                return;

            visualBorder.localScale = size + Vector3.one * visualScaleMultiplier;
        }

        private Vector3 GetColliderLocalSize(Collider col)
        {
            Vector3 size;
            switch (col)
            {
                case BoxCollider box:
                    size = box.size;
                    size.y = size.z;

                    if (exitId == ExitId.ExitNorth || exitId == ExitId.ExitSouth)
                        size.x -= 4;
                    return size;

                case SphereCollider sphere:
                    float d = sphere.radius * 2f;
                    return new Vector3(d, d, d);

                case CapsuleCollider capsule:
                    float diameter = capsule.radius * 2f;
                    return capsule.direction switch
                    {
                        0 => new Vector3(capsule.height, diameter, diameter), // X
                        1 => new Vector3(diameter, capsule.height, diameter), // Y
                        2 => new Vector3(diameter, diameter, capsule.height), // Z
                        _ => Vector3.one
                    };

                default:
                    Debug.LogWarning($"[SceneExitZone] Unsupported collider type '{col.GetType().Name}' " +
                                     "for visual auto-scale.");
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Вызывается editor-инструментами (LocationBuilderTool) после программного
        /// изменения размера коллайдера, когда OnValidate не срабатывает автоматически.
        /// </summary>
        public void EditorSyncVisualScale() => SyncVisualScale();

        private void OnDrawGizmosSelected()
        {
            var box = GetComponent<BoxCollider>();
            if (box == null)
                return;

            // Рисуем в локальном пространстве объекта — Gizmos.matrix сам
            // применит transform.position/rotation/scale, поэтому куб
            // корректно отражает реальную ориентацию коллайдера
            // (важно для West/East, где объект повёрнут на ±90° по Y).
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.25f);
            Gizmos.DrawCube(box.center, box.size);

            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.9f);
            Gizmos.DrawWireCube(box.center, box.size);

            // Стрелка направления — тоже в локальных координатах,
            // "наружу" всегда локальный forward (см. LookRotation в тулзе).
            Gizmos.DrawRay(box.center, Vector3.forward * 1.5f);

            // Сбрасываем matrix перед Handles.Label — Handles работает
            // в мировых координатах и не должен наследовать transform.
            Gizmos.matrix = Matrix4x4.identity;

            UnityEditor.Handles.Label(
                transform.position + Vector3.up * (box.size.y * 0.5f + 0.3f),
                $"{exitId} → {resultStatus}/{resultReason}");
        }
#endif
    }
}
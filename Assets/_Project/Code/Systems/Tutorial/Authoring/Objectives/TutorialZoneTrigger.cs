
using UnityEngine;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Presentation;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Gameplay.Tutorial
{
    /// <summary>Generic зона: игрок вошёл → EventBus. Не знает про шаги/задания.
    /// Одноразовая, но срабатывает только когда объектив её реально ждёт.
    /// Для стрелки/подсветки повесьте на этот же объект WorldTutorialTargetBehaviour с тем же id.</summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TutorialZoneTrigger : MonoBehaviour
    {
        [Tooltip("Пусто = берётся из WorldTutorialTargetBehaviour на этом же объекте.")] [SerializeField]
        private TutorialTargetId targetIdOverride;

        private const string PlayerTag = "Player";
        private TutorialTargetId _targetId;
        private bool _fired;

        private void Reset() => GetComponent<BoxCollider>().isTrigger = true;

        private void Awake()
        {
            var worldTarget = GetComponent<WorldTutorialTargetBehaviour>();
            _targetId = worldTarget != null && worldTarget.TargetId != null
                ? worldTarget.TargetId
                : targetIdOverride;

            if (_targetId == null)
                Debug.LogError(
                    $"[TutorialZoneTrigger] '{name}': targetId не задан ни здесь, ни в WorldTutorialTargetBehaviour.");
        }

        private void OnTriggerEnter(Collider other) => TryFire(other);
        private void OnTriggerStay(Collider other) => TryFire(other);

        private void TryFire(Collider other)
        {
            if (_fired || _targetId == null || !other.CompareTag(PlayerTag)) return;
            if (!TutorialZoneTracker.IsAwaited(_targetId)) return;

            _fired = true;
            EventBus<TutorialZoneEnteredEvent>.Raise(new TutorialZoneEnteredEvent(_targetId));
        }
    }
}
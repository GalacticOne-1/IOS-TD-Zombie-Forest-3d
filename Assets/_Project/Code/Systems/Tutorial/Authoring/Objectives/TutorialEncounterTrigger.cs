using UnityEngine;
using Galactic1.Code.Gameplay.Enemies.Spawning;
using Galactic1.Code.Gameplay.Enemies.Spawning.Requests;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Gameplay.Tutorial
{
    /// <summary>
    /// Тонкий generic scene-триггер "когда игрок вошёл в зону — активируй encounter" —
    /// раздел 18 ТЗ Chapter 2. НЕ знает про tutorial-шаги/главы/id, НЕ вызывает
    /// TutorialService.NextStep() — только спавнит через существующий EnemySpawnSystem
    /// (раздел 17.1: используем существующий Enqueue-API, новый метод в EnemySpawnSystem
    /// не добавлялся, он и так достаточен). Дальше EnemyKilledEvent → EnemyKilledObjective
    /// решает завершение шага — этот класс о завершении шага ничего не знает.
    ///
    /// Одноразовая активация (_activated) — повторный вход в триггер после срабатывания
    /// ничего не делает.
    ///
    /// Резолвит EnemySpawnSystem через ServiceLocator — сервис регистрируется туда
    /// RaidInProgressState.Enter() (см. её правку, добавляющую ServiceLocator.Current.
    /// Register(spawnSystem) рядом с существующим _container.RegisterInstance(spawnSystem)).
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TutorialEncounterTrigger : MonoBehaviour
    {
        [SerializeField] private TutorialEncounterDefinition encounter;

        private const string PlayerTag = "Player";
        private bool _triggered;

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered || !other.CompareTag(PlayerTag)) 
                return;

            _triggered = true;
            Activate();
        }

        private void Activate()
        {
            if (encounter == null || encounter.group == null)
            {
                Debug.LogError($"[TutorialEncounterTrigger] '{name}': encounter/group не задан.");
                return;
            }

            Debug.LogError("Zombie trigger!");

            // var spawnSystem = ServiceLocator.Current.Get<EnemySpawnSystem>();
            // if (spawnSystem == null)
            // {
            //     Debug.LogError($"[TutorialEncounterTrigger] '{name}': EnemySpawnSystem недоступен " +
            //                     "через ServiceLocator — encounter не заспавнен. Проверь регистрацию " +
            //                     "в RaidInProgressState.Enter().");
            //     return;
            // }
            //
            // var origin = transform.position;
            //
            // foreach (var entry in encounter.group.Enemies)
            // {
            //     if (entry.Enemy == null) continue;
            //
            //     for (int i = 0; i < entry.Count; i++)
            //     {
            //         var offset = new Vector3(
            //             Random.Range(-encounter.spawnRadius, encounter.spawnRadius),
            //             0f,
            //             Random.Range(-encounter.spawnRadius, encounter.spawnRadius));
            //
            //         spawnSystem.Enqueue(new EnemySpawnRequest(
            //             entry.Enemy.Id,
            //             origin + offset,
            //             source: SpawnSource.Static));
            //     }
            // }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (encounter == null) return;
            Gizmos.color = new Color(0.9f, 0.1f, 0.1f, 0.25f);
            Gizmos.DrawSphere(transform.position, encounter.spawnRadius);
        }
#endif
    }
}

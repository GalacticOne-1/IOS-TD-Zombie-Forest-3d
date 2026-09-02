
using Galactic1.Code.Gameplay.Units.Zombie;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Собирает визуальную часть врага.
    ///
    /// Gameplay prefab (EnemyInstance) остаётся неизменным.
    /// Visual prefab инстанцируется внутрь VisualRoot.
    /// </summary>
    public sealed class EnemyVisualAssembler
    {
        public void Apply(
            EnemyInstance instance,
            EnemyPresentationDefinition presentation)
        {
            if (instance == null)
            {
                Debug.LogError("[EnemyVisualAssembler] Instance == null");
                return;
            }

            if (presentation == null)
            {
                Debug.LogError("[EnemyVisualAssembler] Presentation == null");
                return;
            }

            if (string.IsNullOrEmpty(presentation.VisualPrefabId))
            {
                Debug.LogError("[EnemyVisualAssembler] PrefabId is empty");
                return;
            }
            
            
            var visualPrefab = Resources.Load<GameObject>($"{AppConstants.PATH_ENEMIES}{presentation.VisualPrefabId}");
            if (visualPrefab == null)
            {
                visualPrefab = Resources.Load<GameObject>($"{AppConstants.PATH_ENEMIES}{presentation.VisualBasePrefabId}");
                Debug.LogError($"[EnemyVisualAssembler] not exist prefab {presentation.VisualPrefabId}");
            }

            var visual = visualPrefab.CreateGO(instance.VisualRoot);

            if (visual == null)
            {
                Debug.LogError($"[EnemyVisualAssembler] Failed create visual prefab: {presentation.VisualPrefabId}");
                return;
            }

            var tr = visual.transform;

            tr.localPosition = Vector3.zero;
            tr.localRotation = Quaternion.identity;
            tr.localScale = Vector3.one;

            var animator = visual.GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogWarning(
                    $"[EnemyVisualAssembler] Animator not found in visual prefab '{presentation.VisualPrefabId}'");
            }
        }
    }
}
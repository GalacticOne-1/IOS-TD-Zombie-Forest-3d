
using Galactic1.Code.Gameplay.Enemies.Definitions;
using Galactic1.Code.Gameplay.Enemies.Variants;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Factories
{
    public sealed class EnemyPresentationFactory
    {
        public EnemyPresentationDefinition Build(
            EnemyPresentationDefinitionData data,
            EnemyVariantResolveResult resolveResult)
        {
            var locomotion = PickLocomotion(data);
            var visualPrefabId = ResolveVisualPrefabId(data, resolveResult);

            return new EnemyPresentationDefinition(
                data.GameplayPrefabId,
                visualPrefabId,
                data.BaseVisualId,
                data.Animator,
                locomotion,
                data.AnimationVariants?.Entries,
                data.Avatar,
                data.AnimationVariants?.name ?? string.Empty);
        }

        private static string ResolveVisualPrefabId(
            EnemyPresentationDefinitionData data,
            EnemyVariantResolveResult result)
        {
            switch (result.Status)
            {
                case VariantResolveStatus.Resolved:
                case VariantResolveStatus.FallbackUsed:
                    return result.ResolvedVisualPrefabId;

                case VariantResolveStatus.DefaultRequired:
                    if (!string.IsNullOrEmpty(data.GameplayPrefabId))
                        return data.GameplayPrefabId;
                    Debug.LogWarning(
                        "[EnemyPresentationFactory] BasePrefabId не задан.");
                    return string.Empty;

                default:
                    Debug.LogError(
                        $"[EnemyPresentationFactory] Неизвестный статус: {result.Status}.");
                    return data.GameplayPrefabId;
            }
        }

        private static AnimatorOverrideController PickLocomotion(
            EnemyPresentationDefinitionData data)
        {
            var controllers = data.LocomotionSet?.Controllers;
            if (controllers == null || controllers.Length == 0)
            {
                Debug.LogWarning("[EnemyPresentationFactory] LocomotionSet пуст.");
                return null;
            }

            return controllers[Random.Range(0, controllers.Length)];
        }
    }
}
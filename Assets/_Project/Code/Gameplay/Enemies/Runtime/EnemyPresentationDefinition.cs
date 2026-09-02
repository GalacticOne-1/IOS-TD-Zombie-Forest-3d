using System.Collections.Generic;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Code.Systems.Raid.Enemies
{
    public sealed class EnemyPresentationDefinition
    {
        public string GameplayPrefabId { get; }
        public string VisualPrefabId { get; }
        public string VisualBasePrefabId { get; }

        public RuntimeAnimatorController Animator { get; }
        public AnimatorOverrideController OverrideController { get; }

        public IReadOnlyList<AnimationVariantConfig.Entry> AnimationVariants { get; }

        public Avatar Avatar { get; }

        public string VariantConfigId { get; }

        public EnemyPresentationDefinition(
            string gameplayPrefabId,
            string visualPrefabId,
            string visualBasePrefabId,
            RuntimeAnimatorController animator,
            AnimatorOverrideController overrideController,
            IReadOnlyList<AnimationVariantConfig.Entry> animationVariants,
            Avatar avatar,
            string variantConfigId)
        {
            GameplayPrefabId = gameplayPrefabId;
            VisualPrefabId = visualPrefabId;
            VisualBasePrefabId = visualBasePrefabId;

            Animator = animator;
            OverrideController = overrideController;
            AnimationVariants = animationVariants;
            Avatar = avatar;
            VariantConfigId = variantConfigId;
        }
    }
}
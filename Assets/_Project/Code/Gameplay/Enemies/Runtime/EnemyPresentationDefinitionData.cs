
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Audio.Voice;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Definitions
{
    /// <summary>
    /// Снапшот визуального представления архетипа ДО резолюции конкретного скина.
    /// EnemyVariantResolver выбирает тему и генерирует PrefabId из Themes[].
    /// </summary>
    public sealed class EnemyPresentationDefinitionData
    {
        /// <summary>Базовый gameplay prefab. Используется если Themes пуст.</summary>
        public string GameplayPrefabId { get; }
        public string BaseVisualId { get; }

        public RuntimeAnimatorController Animator { get; }
        public ZombieLocomotionVariantSet LocomotionSet { get; }
        public AnimationVariantConfig AnimationVariants { get; }
        public Avatar Avatar { get; }
        public VoiceAudioConfig AudioConfig { get;}

        /// <summary>
        /// Тематические наборы скинов.
        /// Каждая тема описывает префикс + количество вариантов.
        /// EnemyVariantResolver фильтрует по правилам локации и выбирает одну тему.
        /// </summary>
        public IReadOnlyList<EnemyThemePresentationDefinition> Themes { get; }

        public EnemyPresentationDefinitionData(
            string gameplayPrefabId,
            string baseVisualId,
            RuntimeAnimatorController animator,
            ZombieLocomotionVariantSet locomotionSet,
            AnimationVariantConfig animationVariants,
            Avatar avatar,
            VoiceAudioConfig audioConfig,
            IReadOnlyList<EnemyThemePresentationDefinition> themes)
        {
            GameplayPrefabId = gameplayPrefabId;
            BaseVisualId = baseVisualId;
            Animator = animator;
            LocomotionSet = locomotionSet;
            AnimationVariants = animationVariants;
            Avatar = avatar;
            AudioConfig = audioConfig;
            Themes = themes;
        }
    }
}
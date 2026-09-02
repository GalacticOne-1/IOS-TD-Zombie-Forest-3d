
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Audio.Voice;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Game.Meta.Enemy
{
    [CreateAssetMenu(
        fileName = "EnemyPresentation",
        menuName = "Game Configs/Enemy/Enemy Presentation")]
    public sealed class EnemyPresentationConfig : ScriptableObject
    {
        [Tooltip("Базовый gameplay prefab. Один на архетип.")]
        public string BasePrefabId;

        [Tooltip("Базовый visual prefab. Используется если Themes пуст.")]
        public string BaseVisualId;

        public RuntimeAnimatorController Animator;
        public ZombieLocomotionVariantSet LocomotionSet;
        public AnimationVariantConfig AnimationVariants;
        public Avatar Avatar;
        [FormerlySerializedAs("AudioDefinition")] public VoiceAudioConfig audioConfig;
        public GameObject DeathVfx;

        [Header("Визуальные темы")]
        [Tooltip("Описывает доступные наборы скинов.\n" +
                 "Веса хранятся в LocationEnemyVisualRules — не здесь.")]
        public EnemyThemePresentation[] Themes;
    }

    /// <summary>
    /// Описание одного набора скинов для темы.
    /// Weight намеренно отсутствует — вероятность определяет локация.
    /// </summary>
    [System.Serializable]
    public sealed class EnemyThemePresentation
    {
        [Tooltip("Тема. Ключ для сопоставления с весами локации.")]
        public EnemyVisualThemeId Theme;

        [Tooltip("Префикс имени visual prefab.\n" +
                 "Пример: 'civil_' → civil_01, civil_02, ...")]
        public string PrefabPrefix;

        [Tooltip("Количество вариантов скинов. Индексы от 01 до N.")] [Min(1)]
        public int VariantsCount = 1;
    }
}
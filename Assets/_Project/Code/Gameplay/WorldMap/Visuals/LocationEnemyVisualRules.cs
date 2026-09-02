
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.WorldMap.Visuals
{
    /// <summary>
    /// Authoring SO: правила визуальных тем врагов для локации.
    ///
    /// Хранит:
    ///   — какие темы разрешены
    ///   — их веса (вероятности выбора)
    ///
    /// Weight живёт здесь, а не в EnemyThemePresentation.
    /// Один и тот же архетип врага выглядит по-разному
    /// в зависимости от локации — баланс контролирует дизайнер локации.
    ///
    /// Примеры:
    ///   City:              civil=70, police=20, hospital=10
    ///   MilitaryBase:      military=90, civil=10
    ///   PoliceDepartment:  police=80, civil=20
    /// </summary>
    [CreateAssetMenu(
        fileName = "LocationEnemyVisualRules_",
        menuName = "Game Configs/World Map/Location Enemy Visual Rules")]
    public sealed class LocationEnemyVisualRules : ScriptableObject
    {
        [Tooltip("Темы и их веса для этой локации.\n" +
                 "Только перечисленные темы будут спавниться.\n" +
                 "Пустой список = все темы разрешены с равным весом.")]
        [SerializeField]
        private List<EnemyThemeWeight> _themeWeights = new();

        /// <summary>
        /// Строит словарь ThemeId → Weight для Definition-слоя.
        /// Вызывается только LocationDefinitionBuilder.
        /// </summary>
        public Dictionary<EnemyVisualThemeId, float> BuildThemeWeightMap()
        {
            var map = new Dictionary<EnemyVisualThemeId, float>();

            foreach (var entry in _themeWeights)
            {
                if (entry.Theme == null) continue;
                map[entry.Theme] = Mathf.Max(0.01f, entry.Weight);
            }

            return map;
        }
    }

    /// <summary>
    /// Пара тема + вес для Inspector-а.
    /// Weight определяет вероятность выбора темы на данной локации.
    /// </summary>
    [System.Serializable]
    public sealed class EnemyThemeWeight
    {
        [Tooltip("Тема врага.")] public EnemyVisualThemeId Theme;

        [Tooltip("Вес выбора (не обязательно в процентах — система нормализует).\n" +
                 "Пример: civil=70, police=20, hospital=10")]
        [Min(0.01f)]
        public float Weight = 1f;
    }
}
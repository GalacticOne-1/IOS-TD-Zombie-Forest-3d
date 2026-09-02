using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Systems.Construction.Configs
{
    /// <summary>
    /// Конфиг системы строительства.
    /// Содержит категории вкладок.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ConstructionConfig",
        menuName = "Game Configs/Construction/Construction Config")]
    public class ConstructionConfig : ScriptableObject
    {
        [field: SerializeField] public List<ConstructionCategoryConfig> Categories { get; private set; }


    }
}
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.RaidLoot.Authoring;
using UnityEngine;

namespace Galactic1.RaidLoot.Definitions
{
    /// <summary>
    /// Визуальный набор для одного типа контейнера (тема/скин).
    /// Хранит префабы трёх состояний: закрыт, открыт, опустошён.
    /// Не содержит логики — только данные.
    ///
    /// Примеры: ForestCrate, MilitaryCrate, LaboratoryCrate.
    /// </summary>
    [CreateAssetMenu(
        fileName = "LootContainerVisualConfig",
        menuName = "Game Configs/Loot/Loot Container Visual Config")]
    public sealed class LootContainerVisualConfig : ScriptableObject
    {
        [SerializeField] private LootVisualId _id;

        [Header("State prefabs")] [Tooltip("Визуал для закрытого состояния")] [SerializeField] 
        private GameObject _closedVisualPrefab;


        public LootVisualId Id => _id;
        public GameObject ClosedVisualPrefab => _closedVisualPrefab;
    }
}
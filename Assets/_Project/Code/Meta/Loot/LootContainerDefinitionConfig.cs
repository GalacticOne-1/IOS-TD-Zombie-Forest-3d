using Galactic1.Core.Enums;
using Galactic1.RaidLoot.Enums;
using UnityEngine;

namespace Galactic1.RaidLoot.Authoring
{
    [CreateAssetMenu(
        fileName = "LootContainerDefinitionConfig",
        menuName = "Game Configs/Loot/Container Definition Config")]
    public class LootContainerDefinitionConfig : ScriptableObject
    {
        [SerializeField] private LootContainerId _id;
        [SerializeField] private ContainerType _containerType;
        [SerializeField] private LootTableConfig _lootTableConfig;


        [Tooltip("Тир контейнера — влияет на tier-фильтр слотов.")] [SerializeField]
        private Tier _containerTier = Tier.T1;



        [Space(20)] [SerializeField] private float _openRadius = 2f;
        [SerializeField] private float _openTimerDelay = 3f;

        [Header("Visual")]
        [Tooltip("Ссылка на LootContainerVisualDefinition в LootContainerVisualDatabase.")]
        [SerializeField]
        private LootVisualId _visualId;



        public LootContainerId Id => _id;
        public ContainerType ContainerType => _containerType;
        public LootTableConfig LootTableConfig => _lootTableConfig;
        public Tier ContainerTier => _containerTier;

        public float OpenRadius => _openRadius;
        public float OpenTimerDelay => _openTimerDelay;
        public LootVisualId VisualId => _visualId;
    }
}
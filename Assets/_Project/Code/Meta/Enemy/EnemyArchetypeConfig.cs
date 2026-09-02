using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Units;
using UnityEngine;

namespace Galactic1.Game.Meta.Enemy
{
    /// <summary>
    /// High-level enemy archetype definition.
    /// Shared immutable authoring asset.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnemyArchetype",
        menuName = "Game Configs/Enemy/Enemy Archetype")]
    public sealed class EnemyArchetypeConfig : ScriptableObject
    {
        [Header("Identity")]
        public EnemyId Id;
        

        public string DisplayName;

        [TextArea]
        public string Description;

        [Header("Presentation")]
        public EnemyPresentationConfig Presentation;

        [Header("Stats")]
        public EnemyStatsConfig Stats;

        [Header("Combat")]
        public EnemyCombatConfig Combat;

        [Header("AI - Behaviour")]
        public EnemyAIConfig AI;
        
        [Header("Movement")]
        public MovementConfig Movement;

        [Header("AI - Targeting / Memory")] 
        public TargetingConfig Targeting;

        [Header("AI - Perception")]
        public PerceptionConfig Perception;

        [Header("Pack")]
        public ZombiePackConfig Pack;
        

        //[Header("Loot")]
        //public EnemyLootConfig Loot;

        //[Header("Spawn")]
        //public EnemySpawnConfig Spawn;
    }
}
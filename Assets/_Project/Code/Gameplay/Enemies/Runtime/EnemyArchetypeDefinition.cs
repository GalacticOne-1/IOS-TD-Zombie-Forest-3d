
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Systems.Raid.Enemies;

namespace Galactic1.Code.Gameplay.Enemies.Definitions
{
    public sealed class EnemyArchetypeDefinition
    {
        public EnemyId EnemyId { get; }
        public string DisplayName { get; }

        public EnemyPresentationDefinitionData Presentation { get; }

        public EnemyAIDefinition AI { get; }
        public MovementDefinition Movement { get; }
        public TargetingDefinition Targeting { get; }
        public PerceptionDefinition Perception { get; }
        public EnemyPackDefinition Pack { get; }
        public EnemyCombatDefinition Combat { get; }
        public MeleeCombatDefinition Melee { get; }

        // Базовые статы — примитивы, не Config
        public float BaseHealth { get; }
        public float BaseArmor { get; }
        public float BasePoise { get; }
        public float BaseStunResistance { get; }

        public EnemyArchetypeDefinition(
            EnemyId enemyId,
            string displayName,
            EnemyPresentationDefinitionData presentation,
            EnemyAIDefinition ai,
            MovementDefinition movement,
            TargetingDefinition targeting,
            PerceptionDefinition perception,
            EnemyPackDefinition pack,
            EnemyCombatDefinition combat,
            MeleeCombatDefinition melee,
            float baseHealth,
            float baseArmor,
            float basePoise,
            float baseStunResistance)
        {
            EnemyId = enemyId;
            DisplayName = displayName;
            Presentation = presentation;
            AI = ai;
            Movement = movement;
            Targeting = targeting;
            Perception = perception;
            Pack = pack;
            Combat = combat;
            Melee = melee;
            BaseHealth = baseHealth;
            BaseArmor = baseArmor;
            BasePoise = basePoise;
            BaseStunResistance = baseStunResistance;
        }
    }
}

using System.Collections.Generic;
using Galactic1.Code.Gameplay.Enemies.Definitions;
using Galactic1.Code.Gameplay.Enemies.Modifiers;
using Galactic1.Code.Gameplay.Enemies.Spawning.Requests;
using Galactic1.Code.Gameplay.Enemies.Variants;
using Galactic1.Code.Systems.Raid.Enemies;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Spawning
{
    public sealed class EnemySpawnContext
    {
        public EnemySpawnRequest Request { get; set; }
        public EnemyArchetypeDefinition ArchetypeDefinition { get; set; }
        public Vector3 SpawnPosition { get; set; }
        public EnemyVariantResolveResult VariantResult { get; set; }
        public EnemyStatMutationContext MutationContext { get; set; }
        public EnemyRuntimeDefinition RuntimeDefinition { get; set; }
        public List<IEnemyModifier> AppliedModifiers { get; set; } = new();
    }
}
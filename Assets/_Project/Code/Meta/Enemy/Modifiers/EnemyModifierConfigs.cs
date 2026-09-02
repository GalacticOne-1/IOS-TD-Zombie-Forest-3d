using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Game.Meta.Enemy.Modifiers
{
    [CreateAssetMenu(
        fileName = "EnemyModifierConfigs",
        menuName = "Game Configs/Enemy/Enemy Modifier Configs")]
    public class EnemyModifierConfigs : ScriptableObject
    {
        [SerializeField] private List<EnemyModifierConfig> modifierConfigs;

        public List<EnemyModifierConfig> ModifierConfigs => modifierConfigs;
    }
}
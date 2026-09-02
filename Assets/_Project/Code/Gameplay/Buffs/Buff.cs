using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Stats
{
    [CreateAssetMenu(menuName = "Game Configs/Stats/Buff", fileName = "Buff")]
    public class Buff : ScriptableObject
    {
        public BuffId Id;

        [Header("Длительность в секундах")]
        public float duration = 10f;

        [Header("Модификаторы статов")]
        public StatModifier[] modifiers;
    }

}
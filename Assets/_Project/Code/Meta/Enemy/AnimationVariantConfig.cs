using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Animation;
using UnityEngine;

namespace Galactic1.Game.Meta.Enemy
{
    [CreateAssetMenu(
        fileName = "AnimationVariantConfig",
        menuName = "Game Configs/Animation/Variant Config")]
    public sealed class AnimationVariantConfig : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public AnimationVariantType Type;

            [Min(1)]
            public int Count;
        }

        [SerializeField] List<Entry> entries;
        
        public IReadOnlyList<Entry> Entries => entries;
    }
}
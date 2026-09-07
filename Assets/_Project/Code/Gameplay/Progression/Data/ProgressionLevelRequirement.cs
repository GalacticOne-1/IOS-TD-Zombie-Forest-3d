using System;
using UnityEngine;

namespace Galactic1.Code.Systems.Progression
{
    [Serializable]
    public sealed class ProgressionLevelRequirement : UnlockRequirement
    {
        [SerializeField] private int requiredLevel = 1;

        public int RequiredLevel => requiredLevel;

        public override bool IsMet(IUnlockContext context)
            => context.ProgressionLevel >= requiredLevel;
    }
}

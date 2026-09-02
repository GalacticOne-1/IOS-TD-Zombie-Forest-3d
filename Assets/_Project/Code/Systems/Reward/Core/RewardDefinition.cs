using System;

namespace Galactic1.Code.Game.Rewards
{
    /// <summary>
    /// Универсальное описание награды.
    /// Награда = набор операций над экономикой.
    /// </summary>
    [Serializable]
    public class RewardDefinition
    {
        public readonly RewardEntry[] entries;

        public RewardDefinition(RewardEntry[] entries)
        {
            this.entries = entries;
        }
    }
}
using System.Collections.Generic;

namespace Galactic1.Code.Core
{
    /// <summary>
    /// Persistent progression state.
    /// Lives inside GameState, synced through ProgressionProxy exactly like
    /// every other Data/Proxy pair in the project (see GameLoopContextData,
    /// FacilityData, RaidResultData).
    /// </summary>
    [System.Serializable]
    public class ProgressionData
    {
        public int Level { get; set; } = 1;
        public int CurrentXP { get; set; }

        /// <summary>
        /// Guids of unlocked UnlockId assets.
        /// </summary>
        public List<string> UnlockedIds { get; set; } = new();

        /// <summary>
        /// Levels for which a skill choice is owed to the player but has not
        /// been resolved yet (soft-launch placeholder — see ProgressionSkillChoiceService).
        /// </summary>
        public List<int> PendingSkillChoiceLevels { get; set; } = new();
    }
}

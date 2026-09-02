using UnityEngine;

namespace Galactic1.Meta.Configs.Recruitment
{
    [CreateAssetMenu(
        fileName = "RecruitmentSettingsConfig",
        menuName = "Game Configs/Recruitment/Recruitment BasicSettings Config")]
    public class RecruitmentSettingsConfig : ScriptableObject
    {
        [Header("Offer Structure")]
        [field: SerializeField] public int CommonOffersCount { get; private set; }
        [field: SerializeField] public int ExperiencedOffersCount { get; private set; }
        [field: SerializeField] public int SpecialistOffersCount { get; private set; }

        [Header("Level Ranges")]
        [field: SerializeField] public int ExperiencedMinLevel { get; private set; }
        [field: SerializeField] public int ExperiencedMaxLevel { get; private set; }

        [field: SerializeField] public int SpecialistMinLevel { get; private set; }
        [field: SerializeField] public int SpecialistMaxLevel { get; private set; }

        [Header("Refresh")]
        [field: SerializeField] public int RefreshIntervalDays { get; private set; }

        public int TotalOffers =>
            CommonOffersCount +
            ExperiencedOffersCount +
            SpecialistOffersCount;
    }
}
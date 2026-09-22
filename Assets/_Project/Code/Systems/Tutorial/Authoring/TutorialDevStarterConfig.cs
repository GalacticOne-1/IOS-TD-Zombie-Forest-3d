
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    [CreateAssetMenu(
        fileName = "TutorialDevStarter",
        menuName = "Game Configs/Tutorial/Tutorial Dev Starter")]
    public sealed class TutorialDevStarterConfig : ScriptableObject
    {
        [SerializeField] private TutorialCampaignId _campaignId;
        [SerializeField] private TutorialChapterId _chapterId;
        [SerializeField] private bool campLocation;


        public bool CampLocation => campLocation;
        public TutorialCampaignId CampaignId => _campaignId;
        public TutorialChapterId ChapterId => _chapterId;
    }
}

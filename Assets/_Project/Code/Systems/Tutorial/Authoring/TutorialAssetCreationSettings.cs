using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Editor-конфигурация создания Tutorial-ассетов.
    /// Runtime не использует этот класс.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TutorialAssetCreationSettings",
        menuName = "Game Configs/Tutorial/Asset Creation Settings")]
    public sealed class TutorialAssetCreationSettings : ScriptableObject
    {
        [Header("Campaign")]
        [Tooltip("Корневая папка, внутри которой находятся папки компаний: 01, 02, 03 и т.д.")]
        [SerializeField]
        private string campaignFolder =
            "Assets/Resources/Configs/Tutorial/Campaigns";

        [Tooltip("Префикс имени Campaign-ассета.")]
        [SerializeField]
        private string campaignNamePrefix =
            "TutorialCampaign_";

        [Header("Chapter")]
        [Tooltip("Префикс имени Chapter-ассета.")]
        [SerializeField]
        private string chapterNamePrefix =
            "TutorialChapter_";

        [Header("Step")]
        [Tooltip("Название папки для Step-ассетов внутри Campaign.")]
        [SerializeField]
        private string stepFolderName =
            "Steps";

        [Tooltip("Префикс имени Step-ассета.")]
        [SerializeField]
        private string stepNamePrefix =
            "TutorialStep_";

        [Header("IDs")]
        [Tooltip("Название папки с ID-ассетами внутри Campaign.")]
        [SerializeField]
        private string idsFolderName = "_Ids";

        [Tooltip("Префикс имени Campaign ID-ассета.")]
        [SerializeField]
        private string campaignIdNamePrefix = "ID.tutorial.campaign_";

        [Tooltip("Префикс имени Chapter ID-ассета.")]
        [SerializeField]
        private string chapterIdNamePrefix = "ID.tutorial.chapter_";

        [Tooltip("Префикс имени Step ID-ассета.")]
        [SerializeField]
        private string stepIdNamePrefix = "ID.tutorial.step_";

        [Header("Guidance")]
        [Tooltip("Папка для создания Guidance-ассетов.")]
        [SerializeField]
        private string guidanceFolder =
            "Assets/Resources/Configs/Tutorial/Guidance";

        [Tooltip("Префикс имени Guidance-ассета.")]
        [SerializeField]
        private string guidanceNamePrefix =
            "Guidance_";

        [Header("Objective")]
        [Tooltip("Папка для создания Objective-ассетов.")]
        [SerializeField]
        private string objectiveFolder =
            "Assets/Resources/Configs/Tutorial/Objectives";

        [Tooltip("Префикс имени Objective-ассета.")]
        [SerializeField]
        private string objectiveNamePrefix =
            "Objective_";
        
        
        
        

        public string CampaignFolder => campaignFolder;
        public string CampaignNamePrefix => campaignNamePrefix;

        public string ChapterNamePrefix => chapterNamePrefix;

        public string StepFolderName => stepFolderName;
        public string StepNamePrefix => stepNamePrefix;

        public string IdsFolderName => idsFolderName;
        public string CampaignIdNamePrefix => campaignIdNamePrefix;
        public string ChapterIdNamePrefix => chapterIdNamePrefix;
        public string StepIdNamePrefix => stepIdNamePrefix;

        public string GuidanceFolder => guidanceFolder;
        public string GuidanceNamePrefix => guidanceNamePrefix;

        public string ObjectiveFolder => objectiveFolder;
        public string ObjectiveNamePrefix => objectiveNamePrefix;
    }
}
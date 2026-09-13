using System.Collections.Generic;
using System.IO;
using Galactic1.Code.Systems.Tutorial.Authoring;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Tools
{
    public class TutorialManager_MenuHelper
    {
        private TutorialManagerWindow _manager;


        public TutorialManager_MenuHelper(TutorialManagerWindow manager)
        {
            _manager = manager;
        }



        public void CreateNewCampaign()
        {
            if (_manager.assetCreationSettings == null)
            {
                EditorUtility.DisplayDialog(
                    "Missing Settings",
                    "TutorialAssetCreationSettings не назначен.",
                    "OK");

                return;
            }

            string campaignsRootFolder =
                _manager.assetCreationSettings.CampaignFolder;

            if (!EnsureFolderExists(campaignsRootFolder))
            {
                EditorUtility.DisplayDialog(
                    "Missing Campaigns Folder",
                    $"Не удалось создать папку:\n{campaignsRootFolder}",
                    "OK");

                return;
            }

            int campaignNumber = FindFirstFreeCampaignNumber(campaignsRootFolder);

            string campaignFolderName = $"{campaignNumber:00}"; // {_manager.assetCreationSettings.CampaignNamePrefix}

            string campaignFolder = CombineAssetPath(campaignsRootFolder, campaignFolderName);

            if (!EnsureFolderExists(campaignFolder))
            {
                EditorUtility.DisplayDialog(
                    "Failed To Create Campaign Folder",
                    $"Не удалось создать папку:\n{campaignFolder}",
                    "OK");

                return;
            }

            string campaignAssetName = $"{_manager.assetCreationSettings.CampaignNamePrefix}{campaignNumber}";

            string campaignPath = CombineAssetPath(campaignFolder, campaignAssetName + ".asset");

            if (AssetDatabase.LoadAssetAtPath<Object>(campaignPath) != null)
            {
                Debug.LogError($"Campaign asset already exists at path: {campaignPath}");

                return;
            }

            var campaign = ScriptableObject.CreateInstance<TutorialDefinition>();

            campaign.name = campaignAssetName;

            AssetDatabase.CreateAsset(campaign, campaignPath);

            string idsFolder =
                CombineAssetPath(campaignFolder, _manager.assetCreationSettings.IdsFolderName);

            if (!EnsureFolderExists(idsFolder))
            {
                Debug.LogError($"Не удалось создать папку IDs: {idsFolder}");

                AssetDatabase.DeleteAsset(campaignPath);
                return;
            }

            //int campaignIdNumber =
                //FindFirstFreeNumber(idsFolder, _manager.assetCreationSettings.CampaignIdNamePrefix);

            TutorialCampaignId campaignId = CreateCampaignId(idsFolder, campaignNumber);

            campaign.campaignId = campaignId;

            if (_manager.registry != null)
            {
                _manager.registry.campaigns ??= new List<TutorialDefinition>();
                _manager.registry.campaigns.Add(campaign);
                EditorUtility.SetDirty(_manager.registry);
            }

            EditorUtility.SetDirty(campaign);
            EditorUtility.SetDirty(campaignId);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _manager.RefreshCampaignList();
            _manager.SelectCampaign(campaign);
        }

        public void CreateNewChapter()
        {
            if (_manager.selectedCampaign == null)
                return;

            if (_manager.assetCreationSettings == null)
            {
                EditorUtility.DisplayDialog(
                    "Missing Settings",
                    "TutorialAssetCreationSettings не назначен.",
                    "OK");

                return;
            }

            string campaignFolder =
                GetSelectedCampaignFolder();

            if (string.IsNullOrEmpty(campaignFolder))
                return;

            int chapterNumber =
                FindFirstFreeNumber(
                    campaignFolder,
                    _manager.assetCreationSettings.ChapterNamePrefix);

            string defaultName =
                $"{_manager.assetCreationSettings.ChapterNamePrefix}{chapterNumber:00}";

            string path =
                EditorUtility.SaveFilePanelInProject(
                    "Create Tutorial Chapter",
                    defaultName,
                    "asset",
                    "Введите название Tutorial Chapter.",
                    campaignFolder);

            if (string.IsNullOrEmpty(path))
                return;

            path = path.Replace("\\", "/");

            var chapter =
                ScriptableObject.CreateInstance<TutorialChapterDefinition>();

            chapter.name =
                Path.GetFileNameWithoutExtension(path);

            AssetDatabase.CreateAsset(chapter, path);

            string idsFolder =
                GetSelectedCampaignIdsFolder();

            if (!EnsureFolderExists(idsFolder))
            {
                Debug.LogError(
                    $"Не удалось создать папку IDs: {idsFolder}");

                AssetDatabase.DeleteAsset(path);
                return;
            }

            int chapterIdNumber =
                FindFirstFreeNumber(
                    idsFolder,
                    _manager.assetCreationSettings.ChapterIdNamePrefix);

            TutorialChapterId chapterId =
                CreateChapterId(
                    idsFolder,
                    chapterIdNumber);

            chapter.chapterId = chapterId;

            _manager.selectedCampaign.chapters ??=
                new List<TutorialChapterDefinition>();

            _manager.selectedCampaign.chapters.Add(chapter);

            EditorUtility.SetDirty(chapter);
            EditorUtility.SetDirty(_manager.selectedCampaign);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _manager.selectedChapter = chapter;
            _manager.SelectStep(null);

            _manager.RefreshCampaignErrors(_manager.selectedCampaign);
        }

        public void CreateNewStep()
        {
            if (_manager.selectedChapter == null)
                return;

            if (_manager.selectedCampaign == null)
                return;

            if (_manager.assetCreationSettings == null)
            {
                EditorUtility.DisplayDialog(
                    "Missing Settings",
                    "TutorialAssetCreationSettings не назначен.",
                    "OK");

                return;
            }

            int chapterNumber =
                GetSelectedChapterNumber();

            if (chapterNumber <= 0)
            {
                EditorUtility.DisplayDialog(
                    "Invalid Chapter",
                    "Не удалось определить номер текущей Chapter.",
                    "OK");

                return;
            }

            string stepsFolder =
                GetSelectedCampaignStepsFolder();

            if (!EnsureFolderExists(stepsFolder))
            {
                EditorUtility.DisplayDialog(
                    "Missing Steps Folder",
                    $"Не удалось создать папку:\n{stepsFolder}",
                    "OK");

                return;
            }

            string idsFolder =
                GetSelectedCampaignIdsFolder();

            if (!EnsureFolderExists(idsFolder))
            {
                EditorUtility.DisplayDialog(
                    "Missing IDs Folder",
                    $"Не удалось создать папку:\n{idsFolder}",
                    "OK");

                return;
            }

            int stepNumber = FindFirstFreeStepNumber(
                stepsFolder,
                chapterNumber);

            string stepFileName =
                $"{_manager.assetCreationSettings.StepNamePrefix}" +
                $"{chapterNumber:00}_{stepNumber:00}.asset";

            string stepPath =
                CombineAssetPath(
                    stepsFolder,
                    stepFileName);

            if (AssetDatabase.LoadAssetAtPath<Object>(stepPath) != null)
            {
                Debug.LogError($"Step asset already exists at path: {stepPath}");

                return;
            }

            var step = ScriptableObject.CreateInstance<TutorialStepDefinition>();

            step.name = Path.GetFileNameWithoutExtension(stepPath);

            step.chapterId = _manager.selectedChapter.chapterId;

            AssetDatabase.CreateAsset(step, stepPath);

            string stepIdFileName =
                $"{_manager.assetCreationSettings.StepIdNamePrefix}" +
                $"{chapterNumber:00}_{stepNumber:00}.asset";

            string stepIdPath = CombineAssetPath(idsFolder, stepIdFileName);

            if (AssetDatabase.LoadAssetAtPath<Object>(stepIdPath) != null)
            {
                Debug.LogError($"Step ID asset already exists at path: {stepIdPath}");

                AssetDatabase.DeleteAsset(stepPath);
                return;
            }

            TutorialStepId stepId =
                CreateStepId(
                    idsFolder,
                    chapterNumber,
                    stepNumber);

            step.stepId = stepId;

            _manager.selectedChapter.steps ??=
                new List<TutorialStepDefinition>();

            _manager.selectedChapter.steps.Add(step);

            EditorUtility.SetDirty(step);
            EditorUtility.SetDirty(stepId);
            EditorUtility.SetDirty(_manager.selectedChapter);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _manager.SelectStep(step);
            _manager.RefreshCampaignErrors(
                _manager.selectedCampaign);
        }



        private TutorialCampaignId CreateCampaignId(
            string idsFolder,
            int number)
        {
            string fileName = $"{_manager.assetCreationSettings.CampaignIdNamePrefix}{number:00}.asset";

            string path = CombineAssetPath(idsFolder, fileName);

            path = AssetDatabase.GenerateUniqueAssetPath(path);

            var campaignId =
                ScriptableObject.CreateInstance<TutorialCampaignId>();

            campaignId.name =
                Path.GetFileNameWithoutExtension(path);

            AssetDatabase.CreateAsset(campaignId, path);

            return campaignId;
        }

        private TutorialChapterId CreateChapterId(
            string idsFolder,
            int number)
        {
            string fileName = $"{_manager.assetCreationSettings.ChapterIdNamePrefix}{number:00}.asset";

            string path = CombineAssetPath(idsFolder, fileName);

            path = AssetDatabase.GenerateUniqueAssetPath(path);

            var chapterId =
                ScriptableObject.CreateInstance<TutorialChapterId>();

            chapterId.name =
                Path.GetFileNameWithoutExtension(path);

            AssetDatabase.CreateAsset(chapterId, path);

            return chapterId;
        }

        private TutorialStepId CreateStepId(
            string idsFolder,
            int chapterNumber,
            int stepNumber)
        {
            string fileName =
                $"{_manager.assetCreationSettings.StepIdNamePrefix}" +
                $"{chapterNumber:00}_{stepNumber:00}.asset";

            string path =
                CombineAssetPath(
                    idsFolder,
                    fileName);

            var stepId =
                ScriptableObject.CreateInstance<TutorialStepId>();

            stepId.name =
                Path.GetFileNameWithoutExtension(path);

            AssetDatabase.CreateAsset(stepId, path);

            return stepId;
        }





        private bool EnsureFolderExists(string folderPath)
        {
            folderPath = folderPath.Replace("\\", "/");

            if (AssetDatabase.IsValidFolder(folderPath))
                return true;

            string parentFolder =
                Path.GetDirectoryName(folderPath)
                    ?.Replace("\\", "/");

            string folderName =
                Path.GetFileName(folderPath);

            if (string.IsNullOrEmpty(parentFolder) ||
                string.IsNullOrEmpty(folderName))
            {
                return false;
            }

            if (!AssetDatabase.IsValidFolder(parentFolder))
            {
                if (!EnsureFolderExists(parentFolder))
                    return false;
            }

            AssetDatabase.CreateFolder(
                parentFolder,
                folderName);

            return AssetDatabase.IsValidFolder(folderPath);
        }






        public int FindFirstFreeNumber(
            string folder,
            string assetPrefix)
        {
            int number = 1;

            while (true)
            {
                string fileName = $"{assetPrefix}{number:00}.asset";
                string path = CombineAssetPath(folder, fileName);

                if (!AssetDatabase.LoadAssetAtPath<Object>(path))
                    return number;

                number++;
            }
        }

        private int FindFirstFreeCampaignNumber(
            string campaignsRootFolder)
        {
            int number = 1;

            while (true)
            {
                string folderName = $"{number:00}"; // {_manager.assetCreationSettings.CampaignNamePrefix}

                string path =
                    CombineAssetPath(campaignsRootFolder, folderName);

                if (!AssetDatabase.IsValidFolder(path))
                    return number;

                number++;
            }
        }

        private int FindFirstFreeStepNumber(
            string stepsFolder,
            int chapterNumber)
        {
            int stepNumber = 1;

            while (true)
            {
                string fileName =
                    $"{_manager.assetCreationSettings.StepNamePrefix}" +
                    $"{chapterNumber:00}_{stepNumber:00}.asset";

                string path = CombineAssetPath(stepsFolder, fileName);

                if (!AssetDatabase.LoadAssetAtPath<Object>(path))
                    return stepNumber;

                stepNumber++;
            }
        }

        private int FindFirstFreeStepIdNumber(
            string idsFolder,
            int chapterNumber)
        {
            int stepNumber = 1;

            while (true)
            {
                string fileName =
                    $"{_manager.assetCreationSettings.StepIdNamePrefix}" +
                    $"{chapterNumber:00}_{stepNumber:00}.asset";

                string path =
                    CombineAssetPath(idsFolder, fileName);

                if (!AssetDatabase.LoadAssetAtPath<Object>(path))
                    return stepNumber;

                stepNumber++;
            }
        }

        public string CombineAssetPath(
            string folder,
            string fileName)
        {
            return Path.Combine(folder, fileName)
                .Replace("\\", "/");
        }

        public string GetSelectedCampaignFolder()
        {
            if (_manager.selectedCampaign == null)
                return null;

            string campaignAssetPath =
                AssetDatabase.GetAssetPath(_manager.selectedCampaign);

            if (string.IsNullOrEmpty(campaignAssetPath))
                return null;

            return Path.GetDirectoryName(campaignAssetPath)
                ?.Replace("\\", "/");
        }


        private string GetSelectedCampaignStepsFolder()
        {
            if (_manager.assetCreationSettings == null)
                return null;

            string campaignFolder =
                GetSelectedCampaignFolder();

            if (string.IsNullOrEmpty(campaignFolder))
                return null;

            return CombineAssetPath(
                campaignFolder,
                _manager.assetCreationSettings.StepFolderName);
        }


        private string GetSelectedCampaignIdsFolder()
        {
            if (_manager.assetCreationSettings == null)
                return null;

            string campaignFolder =
                GetSelectedCampaignFolder();

            if (string.IsNullOrEmpty(campaignFolder))
                return null;

            return CombineAssetPath(
                campaignFolder,
                _manager.assetCreationSettings.IdsFolderName);
        }

        private int GetSelectedChapterNumber()
        {
            if (_manager.selectedCampaign == null ||
                _manager.selectedChapter == null)
            {
                return 0;
            }

            int index =
                _manager.selectedCampaign.chapters
                    .IndexOf(_manager.selectedChapter);

            return index + 1;
        }
    }
}
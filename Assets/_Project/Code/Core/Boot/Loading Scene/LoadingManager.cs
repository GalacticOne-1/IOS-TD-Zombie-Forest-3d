
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    public class LoadingManager : Singleton<LoadingManager>
    {
        public Image progressBar;
        public TMP_Text tProgressDes;
        public TMP_Text tProgress;
        public TMP_Text tVersion;




        private Dictionary<string, LoadingStep> _stepsMap;
        private string currentStep = "";
        private bool newStep;
        float totalProgress = 0f;
        private bool _realProgressStarted;



        public void Launch(Dictionary<string, LoadingStep> steps)
        {
            _stepsMap = steps;
            totalProgress = 0f;

            progressBar.fillAmount = totalProgress;
            tProgress.text = "0%";
            tProgressDes.text = "...";
        }

        public IEnumerator FakeProgressToFive()
        {
            const float delay = .3f;

            for (int percent = 1; percent <= 5; percent++)
            {
                yield return new WaitForSeconds(delay);

                if (_realProgressStarted)
                    yield break;

                totalProgress = percent / 100f;

                progressBar.fillAmount = totalProgress;
                tProgress.text = $"{percent}%";
            }
        }

        public void NewStepStarted(string key)
        {
            _realProgressStarted = true;
            currentStep = key;
            newStep = true;

            var step = _stepsMap.First(_ => _.Key == currentStep).Value;
            tProgressDes.text = step.Description;
            step.LoadStarted = true;

            var isLastStep = currentStep == _stepsMap.Keys.Last();

            if (isLastStep)
            {
                totalProgress = 0.95f;
            }
            else
            {
                var stepFraction = 1f / _stepsMap.Count;
                totalProgress += stepFraction;
            }

            progressBar.fillAmount = totalProgress;
            tProgress.text = $"{(int)(totalProgress * 100)}%";
        }
        
        public void Complete()
        {
            totalProgress = 1f;
            progressBar.fillAmount = 1f;
            tProgress.text = "100%";
        }
    }


    public class LoadingStep
    {
        public string Description;
        public bool LoadStarted;
        public bool LoadCompleted;
    }


    public class CServiceType
    {
        public const string REMOTE_CONFIG = "RemoteConfig";
        public const string PLAYER_PERMISSION = "PlayerPermissions";
        public const string REGISTER_GLOBAL_SERVICES = "RegisterGlobalServices";
        public const string ANALYTICS = "Analitics";
        public const string IAP = "Iap";
        public const string AD = "Ad";
        public const string LOADING_MAIN_SCENE = "LoadingMainScene";

    }
}
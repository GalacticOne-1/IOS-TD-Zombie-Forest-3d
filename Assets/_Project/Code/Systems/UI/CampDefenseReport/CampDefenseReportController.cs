using System;
using System.Collections.Generic;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.RaidReport
{
    /// <summary>
    /// Контроллер панели отчёта после рейда.
    /// Отвечает только за отображение данных:
    /// - название локации
    /// - список бойцов
    /// - список полученного лута
    /// </summary>
    public class CampDefenseReportController : UIScreenPanel
    {
        [Header("Header")] 
        [SerializeField] private TMP_Text missionStatus;
        [SerializeField] private GameObject continueButton;
        [SerializeField] private TMP_Text resultDes;

        [Header("Survival")] 
        [SerializeField] private ScrollRect scrollSurvivor;
        [SerializeField] private Transform survivorRoot;
        [SerializeField] private GameObject survivorPrefab;
        [SerializeField] private GameObject survDividePrefab;

        [Header("Lost Resources")] 
        [SerializeField] private ScrollRect scrollLostResources;
        [SerializeField] private Transform lostResourcesRoot;
        [SerializeField] private GameObject lostResourcesPrefab;
        
        
        private readonly List<CampLostResourceItemView> _lostViews = new();

        private UIStyleResolver _style;
        private Action _onNext;
        
        
        
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            
            _style = ServiceLocator.Current.Get<UIStyleResolver>();

            continueButton.RegisterButtonClick(OnContinueClick);
        }

        public void Show(RaidReportData data, Action onNext)
        {
            _onNext = onNext;
            gameObject.SetActive(true);

            missionStatus.text = data.RaidResult.IsSuccess.Value
                ? "Defense Successful"
                : "Defense Failed";
            var statusColor = _style.ResolveValueColor(
                ValueRangeType.MissionStatus,
                data.RaidResult.IsSuccess.Value ? 1f : 0f);
            
            missionStatus.color = statusColor;
            missionStatus.gameObject.GetChild(0).CMP_Image().color = statusColor;
            missionStatus.gameObject.GetChild(1).CMP_Image().color = statusColor;

            resultDes.text = MissionResultDes(data.RaidResult.IsSuccess.Value);

            UpdateSurvivors(data.Survivors, data.CampSurvivors);
            UpdateLostResources(data);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            survivorRoot.MakeEmpty();
        }


        private void OnContinueClick()
        {
            _onNext?.Invoke();
            Hide();
        }

        // список юнитов
        private void UpdateSurvivors(
            IReadOnlyList<RaidSurvivorResult> survivors, 
            IReadOnlyList<RaidSurvivorResult> campUnits)
        {
            if (survivors == null || survivors.Count == 0) 
                return;
            
            survivorRoot.MakeHidden();

            GameObject label;
            
            // === squad
            label = survDividePrefab.CreateGO(survivorRoot);
            label.GetChild(1).CMP_Text().text = "Squad";
            
            foreach (var s in survivors)
            {
                var view = survivorPrefab.CreateGO(survivorRoot).GetComponent<RaidSurvivorItemView>();
                view.Bind(s);
            }
            
            
            // === camp units
            if (campUnits.Count > 0)
            {
                label = survDividePrefab.CreateGO(survivorRoot);
                label.GetChild(1).CMP_Text().text = "Camp Survivors";
            
                foreach (var s in campUnits)
                {
                    var view = survivorPrefab.CreateGO(survivorRoot).GetComponent<RaidSurvivorItemView>();
                    view.Bind(s);
                }
            }

            
            
            scrollSurvivor.SetSizeContentLayoutGroup(true, survivorRoot, true, true);
            scrollSurvivor.ScrollRectResetV();
        }



        void UpdateLostResources(RaidReportData data)
        {
            if (!data.HasResourcesLost)
                return;
            
            _lostViews.Clear();

            var label = survDividePrefab.CreateGO(lostResourcesRoot);
            label.GetChild(1).CMP_Text().text = "Lost Resources";
            
            var lost = data.ResourcesLost;
            var l = lost.Count;
            for (int i = 0; i < l; i++)
            {
                
                var view = lostResourcesPrefab.CreateGO(lostResourcesRoot)
                    .GetComponent<CampLostResourceItemView>();
                
                view.Bind(lost[i], _style);
                _lostViews.Add(view);
            }

            scrollLostResources.SetSizeContentLayoutGroup(true, lostResourcesRoot, true, true);
            scrollLostResources.ScrollRectResetV();
        }


        string MissionResultDes(bool success)
            => success
                ? "Your survivors successfully repelled the zombie attack and secured the camp. \nAll stored resources remain safe, no buildings were lost, and no penalties have been applied. \nThe camp is ready for the next expedition."
                
                : "The zombies breached your defenses a portion of the resources stored in your warehouses has been lost.";
    }
}

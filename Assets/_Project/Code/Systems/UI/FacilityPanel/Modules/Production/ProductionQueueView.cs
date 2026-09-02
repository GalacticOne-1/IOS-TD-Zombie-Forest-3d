using System;
using Galactic1.Code.Utility;
using Galactic1.Game.Runtime.Production;
using UnityEngine;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine.UI;

namespace Galactic1.Game.UI.Production
{
    /// <summary>
    /// Отображает 5 слотов очереди и кнопки Take / Skip.
    /// </summary>
    public sealed class ProductionQueueView : MonoBehaviour
    {
        [SerializeField] private Image stationImg;
        [SerializeField] private TMP_Text stationLevelText;
        [SerializeField] private TMP_Text remainingTimeText;
        
        [Space]
        [SerializeField] private ProductionSlotView currentSlot;
        [SerializeField] private Transform slotRoot;
        [SerializeField] private GameObject takeButton;
        [SerializeField] private GameObject skipButton;
        [SerializeField] private TMP_Text skipCostText;


        private UIStyleResolver styleResolver;
        private ProductionSlotView[] slotViews;
        
        
        bool isInitialized;
        
        public event Action<string, ProductionJobState> OnSlotClicked;
        public event Action OnTakeClicked;
        public event Action OnSkipClicked;
        

        private int skipCost;




        private void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;
            
            styleResolver = ServiceLocator.Current.Get<UIStyleResolver>();
            takeButton.RegisterButtonClick(() => OnTakeClicked?.Invoke());
            skipButton.RegisterButtonClick(PaidSkip);
            
            
            // === собираем слоты в один список
            slotViews = new ProductionSlotView[1 + slotRoot.childCount];
            var l = slotViews.Length;
            for (int i = 0; i < l; i++) 
            {
                if (i == 0)
                    slotViews[i] = currentSlot; // active slot
                else
                    slotViews[i] = slotRoot.GetChild(i-1).GetComponent<ProductionSlotView>(); // queue
                
                slotViews[i].OnSlotClicked += HandleSlotClicked;
            }
        }

        public void Bind(FacilityDTO dto)
        {
            Initialize();
            
            var details = dto.Details as ProductionFacilityDetailsDTO;
            
            stationImg.sprite = dto.StationIcon;
            stationLevelText.text = $"Lvl. {dto.Level+1}";
            remainingTimeText.text = details.TotalRemainingTime > 0
                ? TimeUtils.FormatTime(details.TotalRemainingTime)
                : "--";

            // === skip premium
            skipCost = details.SkipCost;
            skipCostText.text = $"{skipCost}";
            skipButton.SetActive(details.HasActiveProduction);
            skipButton.ButtonSetInteractable(details.HasActiveProduction && details.CanSkip);

            // слоты
            var l = slotViews.Length;
            for (int i = 0; i < l; i++)
            {
                if (i < details.Queue.Count)
                    slotViews[i].Bind(details.Queue[i], styleResolver);
                else
                    slotViews[i].Clear();
            }

            takeButton.ButtonSetInteractable(details.HasCompleted);
        }
        
        private void HandleSlotClicked(string jobId, ProductionJobState state)
        {
            OnSlotClicked?.Invoke(jobId, state);
        }


        void PaidSkip()
        {
            var data = new ConfirmPaidPopupData(
                "Speed Up Production",
                "Do you want to finish production?",
                "Finish",
                $"{skipCost}",
                onOk: () => { OnSkipClicked?.Invoke(); },
                onClose: () => {  }
            );

            ServiceLocator.Current.Get<UIManager>().OpenPopup(UIScreenId.ConfirmPaidPopup, data);
        }
    }
}
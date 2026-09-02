
using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Utility;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Stations
{
    public sealed class StationCardView : MonoBehaviour
    {
        [Header("Header")] 
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI levelLabel;
        [SerializeField] private TextMeshProUGUI remainingTimeText;
        [SerializeField] private GameObject cActive;
        [SerializeField] private GameObject cLock;

        [Header("Slots")] 
        [SerializeField] private SlotStatusView mainSlot;
        [SerializeField] private Transform slotsRoot;

        [Header("Alert")] 
        [SerializeField] private GameObject alertRoot;
        [SerializeField] private TextMeshProUGUI alertStorageName, alertStorageDes;

        [Header("Button")] 
        [SerializeField] private BaseUIButton clickButton;

        
        private SlotStatusView[] slotViews;
        private Action<RuntimeId> onClicked;
        private RuntimeId stationId;






        public void SetupSlots()
        {
            // === собираем слоты
            if (slotViews == null || slotViews.Length == 0)
            {
                var l = slotsRoot.childCount;
                slotViews = new SlotStatusView[1 + l];

                slotViews[0] = mainSlot;
                for (int i = 0; i < l; i++)
                {
                    slotViews[i+1] = slotsRoot.GetChild(i).GetComponent<SlotStatusView>();
                }
            }
        }


        public void Render(
            UIStyleResolver styleResolver, 
            StationCardDTO dto, 
            Action<RuntimeId> clickCallback)
        {
            stationId = dto.StationId;
            onClicked = clickCallback;
            
            icon.sprite = dto.Icon;
            icon.gameObject.RegisterButtonClick(ShowStationView);
            
            nameLabel.text = dto.Name;
            //nameLabel.transform.GetChild(0).gameObject.CMP_Text().text =
                //dto.IsBuilt ? "" : "- not buld";
            
            levelLabel.text = $"Level {dto.Level + 1}";
            levelLabel.transform.parent.gameObject.SetActive(dto.IsBuilt);
            
            remainingTimeText.text = dto.TotalRemainingTime > 0
                ? TimeUtils.FormatTime(dto.TotalRemainingTime)
                : "--";

            // затемнение если не построено
            cLock.SetActive(!dto.IsBuilt);
            cActive.SetActive(dto.IsBuilt);
            

            // слоты
            SetupSlots();
            var l = slotViews.Length;
            for (int i = 0; i < l; i++)
            {
                if (i < dto.Slots.Length)
                    slotViews[i].Render(dto.Slots[i], styleResolver);
                else
                    slotViews[i].RenderEmpty();
                
                slotViews[i].gameObject.RegisterButtonClick(ShowStationView);
            }

            // алерт
            alertRoot.SetActive(dto.StorageAlert.ShowAlert);
            if (dto.StorageAlert.ShowAlert)
            {
                alertStorageName.text = $"[{dto.StorageAlert.StorageType}]";
                alertStorageDes.text = "Build storage to automate production output.";
            }

            
        }

        private void ShowStationView() => onClicked?.Invoke(stationId);
    }
}
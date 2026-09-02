
using Galactic1.Code.Utility;
using Galactic1.UI.Core;
using Galactic1.UI.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Stations
{
    /// <summary>
    /// Один слот на карточке станции.
    /// Аналог ProductionSlotView — идентичная логика отображения.
    /// </summary>
    public sealed class SlotStatusView : BaseUIButton
    {
        [SerializeField] private Image           itemImg;
        [SerializeField] private Image           progressBar;    // Fill: Horizontal
        [SerializeField] private TextMeshProUGUI countText;      // "2/5"
        [SerializeField] private TextMeshProUGUI remainingTimeText;
        [SerializeField] private GameObject      completedMarker;


        public void Render(SlotStatusDTO dto, UIStyleResolver styleResolver)
        {
            // Иконка + rarity material — идентично ProductionSlotView.Bind
            itemImg.enabled = true;
            itemImg.sprite   = dto.ItemIcon;
            itemImg.material = styleResolver.ResolveRarityColor(dto.Rarity).Material;

            if (progressBar)
                progressBar.fillAmount = dto.RemainingHours > 0 && dto.TotalHours > 0
                    ? 1f - (float)dto.RemainingHours / dto.TotalHours
                    : 0f;

            // Completed marker
            if (completedMarker)
                completedMarker.SetActive(dto.IsCompleted);

            // Счётчик — идентично ProductionSlotView
            if (!dto.IsCompleted && dto.CompletedCount > 0)
                countText.text = TextBuilder.Start()
                    .Color(Color.green)
                    .Size(100)
                    .Text(dto.CompletedCount)
                    .End()
                    .End()
                    .Text("/")
                    .Size(100)
                    .Text(dto.TotalCount)
                    .End()
                    .ToString();
            else if (dto.IsCompleted)
                countText.text = TextBuilder.Start()
                    .Color(Color.green)
                    .Size(100)
                    .Text(dto.CompletedCount)
                    .End()
                    .ToString();
            else
                countText.text = dto.TotalCount > 0 ? dto.TotalCount.ToString() : string.Empty;

            // Время — идентично ProductionSlotView
            if (remainingTimeText != null)
            {
                remainingTimeText.text = TimeUtils.FormatTime(dto.RemainingHours);
                remainingTimeText.gameObject.SetActive(!dto.IsCompleted && dto.RemainingHours > 0);
            }
        }

        public void RenderEmpty()
        {
            itemImg.enabled = false;
            countText.text = "";
            
            if (progressBar) progressBar.fillAmount = 0f;
            if (completedMarker) completedMarker.SetActive(false);
            if (remainingTimeText) remainingTimeText.gameObject.SetActive(false);
        }
    }
}
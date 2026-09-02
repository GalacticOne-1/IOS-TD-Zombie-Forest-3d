using System;
using Galactic1.Code.Utility;
using Galactic1.Game.Runtime.Production;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Galactic1.Game.UI.Production.DTO;
using Galactic1.UI.Core;
using Galactic1.UI.Text;

namespace Galactic1.Game.UI.Production
{
    /// <summary>
    /// Отображает отдельный слот задания.
    /// </summary>
    public sealed class ProductionSlotView : ButtonUIProgrammatic
    {
        [SerializeField] private TMP_Text remainingTimeText;
        [SerializeField] private Image itemImg, progressBar;
        
        [SerializeField] private TMP_Text countText;
        [SerializeField] private GameObject completedMarker;
        
        
        public event Action<string, ProductionJobState> OnSlotClicked;

        public void Bind(ProductionJobDTO dto, UIStyleResolver styleResolver)
        {
            itemImg.enabled = true;
            itemImg.material = styleResolver.ResolveRarityColor(dto.Rarity).Material;
            itemImg.sprite = dto.Icon;

            progressBar.fillAmount = dto.RemainingHours > 0
                ? 1f - (float)dto.RemainingHours / dto.TotalHours
                : 0;

            var completed = dto.State == ProductionJobState.Completed;
            
            completedMarker.SetActive(completed);
            //countText.text = dto.Amount.ToString();
            
            if (!completed && dto.CompletedStack > 0)
                countText.text = TextBuilder.Start()
                    .Color(Color.green)
                    .Size(100)
                    .Text(dto.CompletedStack)
                    .End() // size
                    .End() // color
                    .Text("/")
                    .Size(100)
                    .Text(dto.TotalStack)
                    .End()
                    .ToString();
            else if (completed)
                countText.text = TextBuilder.Start()
                    .Color(Color.green)
                    .Size(100)
                    .Text(dto.CompletedStack)
                    .End()
                    .ToString();
            else
                countText.text = $"{dto.TotalStack}";
            
            
            if(remainingTimeText != null)
            {
                remainingTimeText.text = TimeUtils.FormatTime(dto.RemainingHours);
                remainingTimeText.gameObject.SetActive(!completed);
            }


            gameObject.RegisterButtonClick(() => OnSlotClicked?.Invoke(dto.JobId, dto.State));
        }

        public void Clear()
        {
            //ServiceLocator.Current.Get<VisualMaterialService>().UnregisterGraphic(itemImg);
            itemImg.enabled = false;
            progressBar.fillAmount = 0;
            completedMarker.SetActive(false);
            countText.text = "";
            remainingTimeText?.gameObject.SetActive(false);
        }
    }
}
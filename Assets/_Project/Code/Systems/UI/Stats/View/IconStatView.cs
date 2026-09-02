
using Galactic1.Game.UI.Stats.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Game.UI.Stats
{
    public class IconStatView : StatViewBase
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text value;

        public override void Bind(StatDtoBase data)
        {
            base.Bind(data);
            
            if (data is StatViewDto dto)
            {
                StatUIBuilder.Apply(dto.Entry, null, icon, value, dto.Value);
            }
        }

        public override void ResetView()
        {
            icon.sprite = null;
            value.text = "";
        }
    }
}

using Galactic1.Game.UI.Stats.DTO;
using TMPro;
using UnityEngine;

namespace Galactic1.Game.UI.Stats
{
    public class SimpleStatView : StatViewBase
    {
        [SerializeField] private TMP_Text value;

        public override void Bind(StatDtoBase data)
        {
            base.Bind(data);
            
            if (data is StatViewDto dto)
                StatUIBuilder.Apply(dto.Entry, null, null, value, dto.Value);
        }

        public override void ResetView()
        {
            value.text = "";
        }
    }
}
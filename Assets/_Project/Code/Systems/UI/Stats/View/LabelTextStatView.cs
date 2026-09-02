
using Galactic1.Game.UI.Stats.DTO;
using TMPro;
using UnityEngine;

namespace Galactic1.Game.UI.Stats
{
    public class LabelTextStatView : StatViewBase
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Text value;

        public override void Bind(StatDtoBase data)
        {
            base.Bind(data);
            
            if (data is StatViewDto stat)
            {
                StatUIBuilder.Apply(stat.Entry, label, null, value, stat.Value);
            }
            
            if (data is DescriptorViewDto descriptor)
            {
                StatUIBuilder.Apply(descriptor.StatEntry, label, null, value, 0, descriptor.Value);
            }
        }

        public override void ResetView()
        {
            label.text = "";
            value.text = "";
        }
    }
}
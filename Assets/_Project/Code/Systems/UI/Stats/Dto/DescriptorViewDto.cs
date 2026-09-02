using Galactic1.UI.Core;

namespace Galactic1.Game.UI.Stats.DTO
{
    public class DescriptorViewDto : StatDtoBase, IStatTextDto
    {
        public readonly DescriptorStyleEntry DescriptorEntry;
        public readonly string Label;
        public readonly string Value;
        
        public readonly StatStyleEntry StatEntry; // нужен если поле не статичное
        public StatStyleEntry StatStyleEntry => StatEntry;

        
        
        public DescriptorViewDto(
            DescriptorStyleEntry descriptorEntry,
            (string label, string value) extra,
            StatStyleEntry statEntry = null)
            : base(descriptorEntry.layoutType)
        {
            DescriptorEntry = descriptorEntry;
            Label = extra.label;
            Value = extra.value;
            StatEntry = statEntry;
        }
    }
}
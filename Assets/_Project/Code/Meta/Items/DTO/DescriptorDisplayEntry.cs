using Galactic1.Core.Enums;

namespace Galactic1.Code.Items
{
    public struct DescriptorDisplayEntry
    {
        public DescriptorId DescriptorId;
        public object RawValue;
        public ValueType ValueType;

        public DescriptorDisplayEntry(
            DescriptorId descriptorId, 
            object rawValue, 
            ValueType valueType)
        {
            DescriptorId = descriptorId;
            RawValue = rawValue;
            ValueType = valueType;
        }
    }

    public enum ValueType
    {
        Int,
        Float,
        String,
        Enum,
        Bool,
        Custom,
        IdList,
    }
}
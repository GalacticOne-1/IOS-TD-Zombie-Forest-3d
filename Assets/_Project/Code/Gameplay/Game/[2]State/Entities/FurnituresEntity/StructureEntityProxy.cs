using Galactic1;
using R3;

namespace Galactic1
{
    public class StructureEntityProxy : EntityProxy
    {

        public readonly ReactiveProperty<int> Level;
        
        public StructureEntityProxy(StructureEntityData origin) : base(origin)
        {
            Level = new(origin.Level);
            Level.Skip(1).Subscribe(_ => origin.Level = _);
        }
    }
}
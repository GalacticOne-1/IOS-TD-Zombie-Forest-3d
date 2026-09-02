using Galactic1.Window;
using R3;

namespace Galactic1.UI.Shop
{
    public class ShopCardProxy : WindowCardProxy
    {
        public readonly ReactiveProperty<int> Limit;
        
        public ShopCardProxy(IAPCardData origin) : base(origin)
        {
            Limit = new(origin.Limit);
            Limit.Skip(1).Subscribe(_ => origin.Limit = _);
        }
    }
}
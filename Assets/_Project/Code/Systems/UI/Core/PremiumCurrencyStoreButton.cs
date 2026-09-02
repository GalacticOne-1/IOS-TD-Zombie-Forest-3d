using Galactic1.UI.Core;
using Galactic1.UI.Shop;

namespace Galactic1.Code.UI.Core
{
    public class PremiumCurrencyStoreButton : BaseUIButton
    {
        public override void Initialize(DIContainer container  = null)
        {
            base.Initialize();

            
            if (container != null)
            {
                events.onClick.AddListener(() => container.Resolve<GameStoreService>().ShowWindow());
            }
        }
    }
}
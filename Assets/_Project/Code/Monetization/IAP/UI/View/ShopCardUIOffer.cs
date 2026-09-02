
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Game.Meta.Items;

namespace Galactic1.UI.Shop
{
    public abstract class ShopCardUIOffer : ShopCardUIBase
    {
        
        //protected abstract void ShowPurchaseScreen();

        
        /// <summary>
        /// Для получения предмета
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ItemConfig GetItem(ItemId id) => GameContent.Items.Get(id);
    }
}
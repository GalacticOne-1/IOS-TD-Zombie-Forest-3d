
namespace Galactic1.Code.UI.Inventory
{
    public interface IInventoryContainer
    {
        BaseInventoryData Inventory { get; }

        /// <summary>
        /// Очистка всех слотов
        /// </summary>
        void ClearSlots();
    }
}
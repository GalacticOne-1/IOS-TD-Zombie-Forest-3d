namespace Galactic1.UI.Shop
{
    /// <summary>
    /// Универсальный контекст покупки
    /// </summary>
    public class PurchaseContext
    {
        public string title;
        public string description;
        public int price;

        /// <summary>
        /// Вызывается ТОЛЬКО после подтверждённой и успешной покупки
        /// </summary>
        public System.Action onConfirm;
    }
}
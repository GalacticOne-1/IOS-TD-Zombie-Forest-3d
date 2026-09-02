namespace Galactic1.Game.Buildings.Proxy
{
    [System.Serializable]
    public class ProductionJobData
    {
        public string JobId;
        public string RecipeId;
        public byte Mode;
        
        public byte State;
        public int TotalHours;
        public int StartWorldHour;
        
        /// <summary>
        /// output одного заказа
        /// </summary>
        public int Amount;

        /// <summary>
        /// сколько заказов всего в слоте
        /// </summary>
        public int CurrentStack;

        /// <summary>
        /// сколько заказов уже выполнено
        /// </summary>
        public int CompletedStack;
        
        /// <summary>
        /// Максимальный размер стека заказов для данного рецепта.
        /// </summary>
        public int MaxStack;
        
    }
}
namespace Galactic1.Code.UI.Interaction
{
    public enum UILayerType
    {
        // добавляются по мере необходимости
        HUD = 0, // <-- база, все панели активны
        
        Inventory = 10,
        
        
        Targeting = 20, // <-- активно только связанное с этим слоем
    }
}
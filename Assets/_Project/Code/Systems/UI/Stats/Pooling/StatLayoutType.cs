namespace Galactic1.Game.UI.Stats
{
    public enum StatLayoutType
    {
        Default = 0,
        
        // === Dynamic pooled
        DescriptionText = 5,
        LabelText = 6,
        PriceText = 7,
        Divider = 20,
        Spacer = 21,
        ItemList = 22,          // for linked items
        
        // === Static (no pooling, already in layout)
        StaticLabel = 50,
    }
}
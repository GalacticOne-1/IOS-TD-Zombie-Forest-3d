namespace Galactic1.RaidLoot.Authoring
{
    /// <summary>
    /// Drop Identity — семантическая метка предмета.
    /// Используется DepletionModel и NormalizationRules.
    /// </summary>
    public enum LootDropTag
    {
        Generic = 0,
        Resource = 1,
        
        Food = 5,
        Medical = 6,
        Fuel = 7,
        
        
        Ammo = 10,
        Armor = 11,
        Weapon = 12,
    }
}
namespace Galactic1.Core.Enums
{
    public enum ItemCategory
    {
        Resource     = 0,   // любые не конечные ресурсы, всё что просто хранится и крафтится
        
        Weapon       = 10,
        Armor        = 11,
        Upgrade      = 12,
        Ammo         = 13,
        Backpack     = 14,
        Consumable   = 20,  // всё что используется (еда/медицина/баффы)
        
        Vehicle      = 30,
        
        Station      = 40,
        Storage      = 41,
        BaseFacility = 42,
        DefenseFacility      = 43,
        
        Blueprint    = 50,
    }
}
namespace Galactic1.RaidLoot.Authoring
{
    public enum LootEconomyCategory
    {
        // =========================
        // Strategic Resources
        // =========================

        FoodSupply = 0,
        WaterSupply = 1,
        Fuel = 2,
        GasCylinder = 3,
        EmptyCylinder = 4,

        // =========================
        // Raw Resources
        // =========================

        Wood = 10,
        Stone = 11,
        Ore = 12,
        Scrap  = 13,
        Cloth = 14,

        // =========================
        // Components
        // =========================

        Mechanical = 20,
        Electronics  = 21,
        Chemicals = 22,
        Plastics = 23,
        RareMinerals = 24,
        IronMetrials = 25,

        // =========================
        // Consumables
        // =========================

        Medical = 30,

        // =========================
        // Combat
        // =========================

        Ammo = 50,
        Weapon = 51,
        Armor = 52,
        Grenade = 53,

        // =========================
        // Progression
        // =========================

        Blueprint = 75,

        // =========================
        // Infrastructure
        // =========================

        Tool = 100,
        ConstructionKit = 101,
        VehiclePart = 102
    }
}
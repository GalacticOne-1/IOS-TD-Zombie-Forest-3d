namespace Galactic1
{
    public enum StatId : ushort
    {
        // =========================================================
        // 0–49  CORE / VITAL
        // =========================================================

        Health = 0,
        Armor = 1,
        Resistance = 2, // Общая сопротивляемость

        Level = 8,
        Tier = 9,
        Experience = 10,

        Weight = 11, // Базовый вес
        Durability = 12,

        // =========================================================
        // 20–39 MOVEMENT
        // =========================================================

        MoveSpeed = 20,
        RunSpeed = 21,
        JumpForce = 22,
        WallJumpForce = 23,
        WallSlideSpeed = 24,

        MovementPenalty = 30, // Замедление от веса/эффектов


        // =========================================================
        // 50–69 INVENTORY / CARRY
        // =========================================================

        InventoryCapacity = 50,
        CarryWeightLimit = 51, // Максимальный переносимый вес
        MaxSquadCapacity = 52,

        // =========================================================
        // 100–149 COMBAT – DEFENSE
        // =========================================================

        Dodge = 100,
        Stability = 101,
        Stealth = 102,

        BlockChance = 103,
        DamageReduction = 104,
        Suppression = 105,

        // =========================================================
        // 120–169 COMBAT – OFFENSE BASE
        // =========================================================

        Damage = 120,
        DamagePerSec = 121,

        Penetration = 122,
        ShredDamage = 123,

        FireDamage = 124,
        IceDamage = 125, // ДОБАВЛЕНО (масштабируемость)
        PoisonDamage = 126, // ДОБАВЛЕНО


        // =========================================================
        // 170–199 COMBAT – CRITICAL
        // =========================================================

        CritChance = 170,
        CritDamage = 171,


        // =========================================================
        // 200–239 COMBAT – RANGED
        // =========================================================

        Accuracy = 200,
        FireRate = 201,
        ReloadSpeed = 202,
        MagazineCapacity = 203,
        AmmoType = 204,

        AttackRange = 205,
        VisionRange = 206,
        HearingRange = 207,

        AoeDamage = 210,
        AoeRange = 211,


        // =========================================================
        // 250–279 ABILITY / ACTIVE EFFECTS
        // =========================================================

        AbilityCooldown = 250,
        HealingPower = 251,
        SlowAmount = 252,
        Duration = 253, // ДОБАВЛЕНО (для эффектов)
        RestoreHealth = 254,


        // =========================================================
        // 300–329 SURVIVAL
        // =========================================================

        Hunger = 300,
        Thirst = 301,
        Stamina = 302, 
        HungerDecay = 303,
        ThirstDecay = 304,

        // =========================================================
        // 350–399 INFRASTRUCTURE / META
        // =========================================================

        LinkedAmmo = 350,
        LinkedWeapons = 351,
        LinkedModules = 352,
        LinkedArmors = 353,
            
        UnitSlots = 400,
        ProductionSpeed = 401, // ДОБАВЛЕНО (если будут здания)
    }
}
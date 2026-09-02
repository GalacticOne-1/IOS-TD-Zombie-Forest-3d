
namespace Galactic1.Core.Results
{
    /// <summary>
    /// Унифицированные причины отказа операций во всей игре.
    /// Используются Runtime-слоем.
    /// </summary>
    public enum NotificationFailReason
    {
        None = 0,

        NotEnoughPremiumCurrency = 1,
        NotEnoughSoftCurrency = 2,

        AdNotAvailable = 3,
        AdBreak = 4,


        NoFreeCampSlots = 20,
        SquadIsEmpty = 21,
        SquadIsDestroyed = 22,

        CargoDroneEmptySlots = 50,
        CargoDroneNotCharge = 51,

        IsHungry = 60,
        IsThirsty = 61,

        Damage = 70,
        Armor = 71,
        Accuracy = 72,
        DPS = 73,


        UnknownError = 200
    }
}
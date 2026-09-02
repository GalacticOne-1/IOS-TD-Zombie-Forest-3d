namespace Galactic1.Code.Gameplay.Damage
{
    public enum TargetSelectionMode
    {
        Closest,    // ближайшая цель
        Weakest,    // цель с наименьшим здоровьем
        Random,     // случайная цель
        HighestPriority // цель с максимальным приоритетом
    }
}
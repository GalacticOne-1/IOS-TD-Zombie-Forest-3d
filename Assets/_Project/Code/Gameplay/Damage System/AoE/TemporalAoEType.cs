namespace Galactic1.Code.Gameplay.AoE
{
    /// <summary>
    /// Тип временной AoE-зоны.
    /// Определяет какой эффект применяется к юнитам внутри зоны.
    /// </summary>
    public enum TemporalAoEType
    {
        /// <summary>Обычный взрыв — зона не создаётся.</summary>
        None = 0,
        
        /// <summary>Молотов / кислотная граната — периодический урон.</summary>
        Burn,
 
        /// <summary>Электрическая граната — урон + замедление.</summary>
        Electric,
 
        /// <summary>Шумовая граната — временная остановка, переход в Idle.</summary>
        Concussive
    }
}
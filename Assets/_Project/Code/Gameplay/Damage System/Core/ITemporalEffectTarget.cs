namespace Galactic1.Code.Gameplay.AoE
{
    /// <summary>
    /// Контракт для объектов, на которых зональные гранаты применяют эффекты.
    ///
    /// Реализуется SurvivorInstance (и любым будущим юнитом/сущностью).
    /// TemporalAoEZone работает только через этот интерфейс —
    /// не знает про SurvivorInstance напрямую.
    /// </summary>
    public interface ITemporalEffectTarget
    {
        /// <summary>Применить замедление (Electric-граната).</summary>
        void ApplySlowEffect(float walkMultiplier, float runMultiplier);

        /// <summary>Снять замедление при выходе из зоны.</summary>
        void RemoveSlowEffect();

        /// <summary>Применить стан на заданное время (Concussive-граната).</summary>
        void ApplyStunEffect(float duration);
    }
}
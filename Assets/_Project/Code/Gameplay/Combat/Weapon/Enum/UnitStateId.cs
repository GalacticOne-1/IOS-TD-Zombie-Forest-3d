namespace Galactic1.Code.Gameplay.Units
{
    public enum UnitStateId
    {
        Idle,
        SquadMoving,
        Engaging,
        MeleeEngaging,
        UsingAbility,
        Suppressed,
        Panicking,
        Dying,
        Dead,
        
        
        
        
        // ── Player-only ────────────────────────────────────
        TakingCover,
        CoverMoving,
 
        // ── AI (Zombie / Boss) ─────────────────────────────
        /// <summary>
        /// Патрулирование без цели. Walk-анимация, случайные waypoints.
        /// Переход в Chasing при обнаружении игрока.
        /// </summary>
        Roaming,
 
        /// <summary>
        /// Активная погоня за целью. Run-анимация, повышенная скорость.
        /// Переход в MeleeEngaging при входе в AttackRange.
        /// </summary>
        Chasing,
 
        // ── Boss-only (Phase 4) ────────────────────────────
        PhaseTransition,
    }
}
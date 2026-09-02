using Galactic1.Code.Gameplay.Combat.Data;

namespace Galactic1.Code.Gameplay.Combat.Burst
{
    /// <summary>
    /// Container for a batch of hit requests belonging to one fire action.
    ///
    /// NOTE: Plain managed array for Phase 1 — NOT NativeArray/Burst/Jobs yet.
    /// Named "Native" to signal intent: this is the seam where a future
    /// migration to Unity.Collections.NativeArray + Burst-compiled jobs
    /// will happen once profiling shows it's needed (shotgun/minigun pellet
    /// counts are currently small enough that managed arrays are fine).
    ///
    /// Used by BurstFireResolver → HitResolver.
    /// </summary>
    public sealed class NativeHitBatch
    {
        public HitRequest[] Requests;

        public NativeHitBatch(HitRequest[] requests)
        {
            Requests = requests;
        }
    }
}
using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// RAW снимок состояния мира + aggro memory для одного think-тика.
    ///
    /// Неизменен на протяжении тика. Evaluate() только читает.
    ///
    /// Содержит:
    ///   Perception  — что видит юнит прямо сейчас
    ///   AggroMemory — последняя известная цель (из Blackboard)
    ///   Noise       — слышит ли шум (из Blackboard)
    ///   Alert       — behavioral phase (из Blackboard)
    /// </summary>
    public  class AIContext
    {
        // ── Perception (visible right now) ────────────────────────────────

        /// <summary>Ближайшая видимая враждебная цель. null если не видит.</summary>
        public ITargetInfo VisibleTarget;

        public Vector3 VisibleTargetPosition;
        public float DistanceToVisibleTarget;
        public bool HasVisibleTarget;
        public float VisibleTargetHealthNormalized;

        // ── Aggro memory ──────────────────────────────────────────────────

        /// <summary>true если есть aggro-цель (видимая или в памяти).</summary>
        public bool HasAggroTarget;

        /// <summary>Последняя известная позиция aggro-цели.</summary>
        public Vector3 LastKnownTargetPosition;

        /// <summary>Секунд с момента последнего обнаружения цели.</summary>
        public float TimeSinceSawTarget;

        /// <summary>
        /// true если цель была видна недавно (TimeSinceSawTarget < LoseTargetDelay).
        /// Actions используют это для SearchAction score.
        /// </summary>
        public bool IsTargetInMemory;

        // ── Noise ─────────────────────────────────────────────────────────

        public bool HeardNoise;
        public Vector3 NoisePosition;
        public float NoiseIntensity;

        // ── Alert phase ───────────────────────────────────────────────────

        public AlertPhase AlertPhase;

        // ── FSM ───────────────────────────────────────────────────────────

        public UnitStateId CurrentState;

        // ── Time ──────────────────────────────────────────────────────────

        public float DeltaTime;
    }
}
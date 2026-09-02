using Galactic1.Code.Gameplay.Units.Brain.Zombie;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Blackboard
{
    /// <summary>
    /// Per-unit изменяемая память AI.
    ///
    /// Разделена на секции по ответственности:
    ///   Pack      — pack-слот и его позиция
    ///   Aggro     — текущая цель и memory
    ///   Noise     — услышанный шум
    ///   Combat    — cooldown атаки
    ///   Alert     — behavioral phase (Calm / Suspicious / Alerted / Combat)
    ///   Hysteresis — защита от flickering между состояниями
    /// </summary>
    public class EnemyBlackboard
    {
        // ── Pack ──────────────────────────────────────────────────────────

        public readonly PackReservationService PackReservation;
        public string ReservedTargetId;
        public Vector3 PackSlotPosition;

        // ── Aggro memory ──────────────────────────────────────────────────

        /// <summary>
        /// ID текущей aggro-цели. null = нет цели.
        /// Обновляется AIContextBuilder когда цель видна.
        /// Сохраняется LoseTargetDelay секунд после потери видимости.
        /// </summary>
        public string AggroTargetId;

        /// <summary>Последняя известная позиция aggro-цели.</summary>
        public Vector3 LastKnownTargetPosition;

        /// <summary>Time.time когда цель была видна последний раз.</summary>
        public float LastTimeSawTarget;

        /// <summary>
        /// Время с момента последнего обнаружения.
        /// Вычисляется в AIContextBuilder: Time.time - LastTimeSawTarget.
        /// </summary>
        public float TimeSinceSawTarget;

        // ── Noise ─────────────────────────────────────────────────────────

        /// <summary>true = зомби услышал звук и ещё не исследовал позицию.</summary>
        public bool HeardNoise;

        /// <summary>Позиция услышанного звука.</summary>
        public Vector3 NoisePosition;

        /// <summary>Интенсивность шума [0..1]. Влияет на score InvestigateAction.</summary>
        public float NoiseIntensity;

        /// <summary>Источник шума (опционально). Для damage aggro.</summary>
        public ITargetInfo NoiseSource;

        // ── Combat ────────────────────────────────────────────────────────

        /// <summary>
        /// Секунд до следующей атаки.
        /// Уменьшается в UtilityUnitBrain.Tick(), используется AttackAction.
        /// </summary>
        public float AttackCooldownRemaining;

        // ── Alert phase ───────────────────────────────────────────────────

        /// <summary>
        /// Текущая behavioral phase.
        /// Не отдельный FSM — только blackboard flag для score modifiers.
        /// </summary>
        public AlertPhase AlertPhase = AlertPhase.Calm;

        // ── Hysteresis ────────────────────────────────────────────────────

        /// <summary>
        /// Минимальное время в текущем AI-решении (секунды).
        /// Защищает от flickering attack ↔ chase каждый think-тик.
        /// </summary>
        public float CommitTimeRemaining;

        /// <summary>StateId последнего выбранного действия. Для commit-проверки.</summary>
        public UnitStateId LastChosenState = UnitStateId.Idle;

        // ── Ctor ──────────────────────────────────────────────────────────

        public EnemyBlackboard(PackReservationService packReservation)
        {
            PackReservation = packReservation
                              ?? throw new System.ArgumentNullException(nameof(packReservation));
        }

        // ── Helpers ───────────────────────────────────────────────────────

        public bool HasPackSlot => PackReservation.HasSlot;
        public bool HasAggroTarget => !string.IsNullOrEmpty(AggroTargetId);
        public bool IsTargetLost => HasAggroTarget && TimeSinceSawTarget > 0f;

        public void ReleasePackSlot(UnitInstance unit)
        {
            PackReservation.Release(unit);
            ReservedTargetId = null;
            PackSlotPosition = Vector3.zero;
        }

        public void ClearAggro()
        {
            AggroTargetId = null;
            LastKnownTargetPosition = Vector3.zero;
            LastTimeSawTarget = 0f;
            TimeSinceSawTarget = 0f;
        }

        public void ClearNoise()
        {
            HeardNoise = false;
            NoisePosition = Vector3.zero;
            NoiseIntensity = 0f;
            NoiseSource = null;
        }
    }

    /// <summary>
    /// Behavioral phases. Blackboard флаги — не отдельный FSM.
    /// Score modifiers читают это для контекстного усиления/ослабления Actions.
    /// </summary>
    public enum AlertPhase
    {
        /// <summary>Зомби спокоен, роуминг.</summary>
        Calm,

        /// <summary>Услышан шум — идёт исследовать.</summary>
        Suspicious,

        /// <summary>Увидел врага или получил урон.</summary>
        Alerted,

        /// <summary>Активное преследование / атака.</summary>
        Combat,
    }
}
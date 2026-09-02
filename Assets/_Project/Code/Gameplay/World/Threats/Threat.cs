using System;
using UnityEngine;

namespace Galactic1.Code.Systems.World.Threats
{
    /// <summary>
    /// Класс, описывающий одну угрозу в мире.
    /// </summary>
    public class Threat
    {
        public string Id { get; }
        public ThreatType Type { get; }
        public ThreatStage Stage { get; private set; }
        

        /// <summary>
        /// День создания угрозы (мир начал процесс)
        /// </summary>
        public int CreatedAtDay { get; }

        /// <summary>
        /// День, когда угроза становится заметной игроку
        /// </summary>
        public int RevealDay { get; }

        /// <summary>
        /// День, когда атака обязана начаться
        /// </summary>
        public int AttackDay { get; }

        
        
        public Threat(
            string id,
            ThreatType type,
            int createdAtDay,
            int revealDay,
            int attackDay,
            ThreatStage stage = ThreatStage.Dormant)
        {
            Id = id;
            Type = type;
            CreatedAtDay = createdAtDay;
            RevealDay = revealDay;
            AttackDay = attackDay;
            Stage = stage;
        }

        public void SetStage(ThreatStage stage)
        {
            Stage = stage;
        }

        /// <summary>
        /// Сколько дней осталось до атаки
        /// </summary>
        public int GetRemainingDays(int currentHours)
        {
            return Math.Max(0, (AttackDay * 24) - currentHours)-24;
        }

        /// <summary>
        /// Активирует угрозу (игрок должен реагировать)
        /// </summary>
        public void Activate() => Stage = ThreatStage.Active;

        /// <summary>
        /// Устраняет угрозу
        /// </summary>
        public void Resolve() => Stage = ThreatStage.Resolved;
    }
}
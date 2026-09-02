
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Gameplay.Player;

namespace Galactic1.Code.Gameplay.Units.Stats
{
    /// <summary>
    /// Контроллер статов игрока
    /// </summary>
    public class RaidSurvivorStatsRuntime : StatsRuntimeBase
    {
        public RaidSurvivorStatsRuntime(
            string _owner,
            Dictionary<StatId, float> baseStats,
            Dictionary<StatId, float> savedCurrent,
            IEquipmentStatsProvider equipmentStatsProvider)
            : base(_owner, baseStats, equipmentStatsProvider)
        {
            // применяем snapshot значений (HP и т.п.)
            foreach (var kvp in savedCurrent)
                SetIfExists(kvp.Key, kvp.Value);
            
            ActivateLive();
            // ******************************************************************************************************
            // ******************************************************************************************************

        }

        public override void ModifyStat(StatId stat, float amount)
        {
            base.ModifyStat(stat, amount);                      // изменить CurrentStats с clamp
            SetControlFeatures(stat);
        }
        public override void SetStat(StatId stat, float amount)
        {
            base.SetStat(stat, amount);                         // изменить CurrentStats с clamp
            SetControlFeatures(stat);
        }
        
        public void SetControlFeatures(StatId stat)
        {
            switch (stat)
            {
                case StatId.MoveSpeed:
                    PlayerControlStatsRepository.speedMovement = CurrentStats[StatId.MoveSpeed];
                    break;
                case StatId.JumpForce:
                    PlayerControlStatsRepository.jumpForce = CurrentStats[StatId.JumpForce];
                    break;
                case StatId.WallJumpForce:
                    PlayerControlStatsRepository.wallJumpForce = CurrentStats[StatId.WallJumpForce];
                    break;
                case StatId.WallSlideSpeed:
                    PlayerControlStatsRepository.wallSlideSpeed = CurrentStats[StatId.WallSlideSpeed];
                    break;
            }
        }
    }
}

using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Gameplay.Player;
using Galactic1.Structs;

namespace Galactic1.Code.Gameplay.Units.Stats
{
    /// <summary>
    /// Контроллер статов игрока
    /// </summary>
    public class SurvivorStatsRuntime : StatsRuntimeBase
    {
        public PlayerProxy Proxy { get; private set; }


        public SurvivorStatsRuntime(
            string _owner,
            PlayerProxy proxy,
            Dictionary<StatId, float> baseStats, 
            IEquipmentStatsProvider equipmentStatsProvider) 
            : base(_owner,baseStats, equipmentStatsProvider)
        {
            
            // прокси игрока
            Proxy = proxy;
            
            // Ссылка на экипировку игрока
            //EquipmentInventory = GetComponent<PlayerEquipmentContainer>().Inventory as PlayerEquipmentInventoryData;
            
            ActivateLive();
            // ******************************************************************************************************
            // ******************************************************************************************************

            OnDeath += () =>
            {
                Proxy.IsDead.Value = true;
#if UNITY_EDITOR
                DLog.Alert($"Unit die: {Proxy.Name}", EDlogColor.ORANGE);
#endif
            };
            
            // обновляем все подписки UI
            foreach (var kvp in Proxy.Stats)
                Proxy.Stats[kvp.Key].ForceNotify();
        }


        protected override void ApplySave()
        {
            SetIfExists(StatId.Health, Proxy.Stats[StatId.Health].Value);
            SetIfExists(StatId.Hunger, Proxy.Stats[StatId.Hunger].Value);
            SetIfExists(StatId.Thirst, Proxy.Stats[StatId.Thirst].Value);
        }
        
        public override void ModifyStat(StatId stat, float amount)
        {
            base.ModifyStat(stat, amount);                      // изменить CurrentStats с clamp
            Proxy.Stats[stat].Value = CurrentStats[stat];       // пушим в Proxy
            SetControlFeatures(stat);
        }
        public override void SetStat(StatId stat, float amount)
        {
            base.SetStat(stat, amount);                         // изменить CurrentStats с clamp
            Proxy.Stats[stat].Value = CurrentStats[stat];       // пушим в Proxy
            SetControlFeatures(stat);
        }
        
        protected override void SyncProxyStats()
        {
            foreach (var stat in _recalculator.DirtyStats)
            {
                if (!Proxy.Stats.ContainsKey(stat))
                    continue;
                
                Proxy.Stats[stat].Value = resourceStats.Contains(stat)
                    ? CurrentStats[stat]
                    : CalculatedStats[stat];
                SetControlFeatures(stat);
            }
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
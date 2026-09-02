
using System;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Systems.Raid.Survivors;
using UnityEngine;

namespace Galactic1.Code.Systems.Raid
{
    /// <summary>
    /// Единая точка входа для боевой логики.
    /// Любой юнит в бою (лагерь или рейд) приводится к этому интерфейсу.
    /// </summary>
    public interface IUnitRuntime : IUnitRuntimeBase
    {
        string ArchetypeId { get; }
        string DisplayName { get; }
        bool IsCampDefender { get; }
        
        IUnitInventoryRuntime InventorySource { get; }
        IEquipmentStatsProvider EquipmentService { get; }
        IUnitWeaponRuntime Weapon { get; }
        
        UnitStatus Status { get; }
        event Action<ItemUseContext> OnAbilityAnimationRequested;
        
        QuickSlotMapping QuickSlot { get; }
        CooldownTracker Cooldowns { get; }
        
        SurvivorGameplayDefinition Definition { get; }
        
        bool IsInCover { get; }

        void RequestAbilityAnimation(ItemUseContext ctx);
        
        EquipmentRuntimeService_Preview GetEquipmentService_Preview();

    }
}
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Game.Meta.Items;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1
{
    
    
    public readonly struct RecruitCompletedEvent : IEvent
    {
        public readonly RecruitCategory Category;
        public RecruitCompletedEvent(RecruitCategory category) => Category = category;
    }

    public sealed class UnitMovedEvent : IEvent {}
    
    
    /// <summary>
    /// Семантическое событие "способность реально исполнена" — раздел 5-7 ТЗ Chapter 2.
    ///
    /// Поднимается ИСКЛЮЧИТЕЛЬНО из AbilityComponent.ExecutePending(), сразу ПОСЛЕ
    /// pending.Behaviour.Execute(pending.Context, pending.Slot) — то есть уже после того,
    /// как GrenadeBehaviour.Execute() реально заспавнил и запустил projectile. Это не то
    /// же самое, что ItemUseContext.OnConfirmed (targeting confirmation в
    /// AbilityUseCoordinator.StartTargeting) — тот срабатывает ДО animation/execution и не
    /// является доказательством фактического использования (см. докстринг
    /// AbilityComponent.ExecutePending).
    ///
    /// UnitId/ItemId — canonical типы проекта (string Id юнита, ItemId предмета), не
    /// строковые дубликаты. Type — ConsumableType, уже существующий enum
    /// (Galactic1.Game.Meta.Items), задан абстрактным свойством ConsumableBehaviour.Type.
    /// </summary>
    public readonly struct AbilityUsedEvent : IEvent
    {
        public readonly string UnitId;
        public readonly ItemId ItemId;
        public readonly ConsumableType Type;

        public AbilityUsedEvent(string unitId, ItemId itemId, ConsumableType type)
        {
            UnitId = unitId;
            ItemId = itemId;
            Type = type;
        }
    }
    
    
    /// <summary>
    /// Поднимается при смерти ЛЮБОГО врага (ambient/wave/director —
    /// источник различается через Runtime.SpawnSource). Единая точка входа
    /// для всех подписчиков (WaveSystem, будущий killed-counter для
    /// RaidResultProxy, аналитика), вместо приватного EnemyRuntime.OnDeath
    /// на каждого подписчика по отдельности.
    /// </summary>
    public sealed class EnemyKilledEvent : IEvent
    {
        public readonly EnemyRuntime Runtime;

        public EnemyKilledEvent(EnemyRuntime runtime)
        {
            Runtime = runtime;
        }
    }

    public struct SurvivorStatusChangedEvent : IEvent
    {
        public readonly string UnitId;
        public readonly bool IsHungry;
        public readonly bool IsThirsty;

        public SurvivorStatusChangedEvent(string unitId, bool isHungry, bool isThirsty)
        {
            UnitId = unitId;
            IsHungry = isHungry;
            IsThirsty = isThirsty;
        }
    }
    
    
    
    // === События смерти юнитов игрока
    public struct UnitKilledEvent : IEvent
    {
        public readonly SurvivorInstance Unit;
        public UnitKilledEvent(SurvivorInstance unit) => Unit = unit;
    }

    public struct UnitReadyForDespawnEvent : IEvent
    {
        public readonly SurvivorInstance Unit;
        public UnitReadyForDespawnEvent(SurvivorInstance unit) => Unit = unit;
    }
//
}
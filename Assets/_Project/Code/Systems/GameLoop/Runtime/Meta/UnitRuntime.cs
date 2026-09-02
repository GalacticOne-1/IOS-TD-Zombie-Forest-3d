using System;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Sources;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Code.Systems.Raid;
using Galactic1.Configs;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.Raid.Survivors;
using Galactic1.Meta.Configs.Recruitment;
using Galactic1.Structs;
using UnityEngine;

namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Runtime-обёртка над PlayerProxy
    /// </summary>
    public class UnitRuntime : 
        InventoryOwnerRuntime, 
        IEquipmentStateListener,
        IUnitRuntime
        
    {
        public readonly PlayerProxy Proxy;
        
        readonly SurvivorStatsRuntime _stats;
        MetaUnitInventoryRuntime _inventorySource { get; }
        readonly EquipmentRuntimeService _equipmentService;
        //private ItemBrokenHandler _itemBrokenHandler;
        
        // === IRuntimeUnit ===
        public string Id => Proxy.Id;
        public string ArchetypeId => Proxy.ArchetypeId;
        public string DisplayName => Proxy.Name.Value;
        public bool IsCampDefender { get; }
        public int TeamId => 0;
        public IUnitStatsRuntime Stats => _stats;
        
        public IUnitInventoryRuntime InventorySource => _inventorySource;
        public IEquipmentStatsProvider EquipmentService => _equipmentService;
        
        
        public SurvivorGameplayDefinition Definition { get; }
        public UnitGameplayDefinition RuntimeDefinition => Definition;
        public UnitStatus Status { get; }
        public IUnitWeaponRuntime Weapon { get; }
        public QuickSlotMapping QuickSlot { get; }
        public ActiveEffectsRuntime Effects { get; }
        public CooldownTracker Cooldowns { get; }
        public bool IsInCombat { get; }
        public bool IsInCover { get; }
        public Vector3 SpawnPosition { get; }
        

        public event Action<ItemUseContext> OnAbilityAnimationRequested;


        public void RequestAbilityAnimation(ItemUseContext ctx) => OnAbilityAnimationRequested?.Invoke(ctx);
        
        

        
        
        /// <summary>
        /// Создается один раз на всю игру
        /// (при старте игры / при создании нового выжившего)
        /// </summary>
        /// <param name="proxy"></param>
        public UnitRuntime(PlayerProxy proxy)
        {
            Proxy = proxy;
            var statsDefault = ServiceLocator.Current.Get<ConfigProvider>().Get<PlayerStatsBase>();
            var consumptionConfig = ServiceLocator.Current.Get<ConfigProvider>().Get<SurvivorConsumptionConfig>();

            
            // #1 inventory
            RegisterInventorySource(new InventoryProxySourceAdapter(
                $"UnitEquip_{Proxy.Id}",
                this,
                ServiceLocator.Current.Get<ConfigProvider>().Get<UnitInventoryEquipmentConfig>(),
                Proxy.EquipmentProxy,
                InventorySourceType.UnitEquipment,
                this));
            _inventorySource = new MetaUnitInventoryRuntime(this);
            QuickSlot = new QuickSlotMapping(InventorySource.Equipment);
            
            // #2 equipment
            _equipmentService = new EquipmentRuntimeService();
            _equipmentService.BindSource(_sources[0]);

            // #3 stats
            _stats = new SurvivorStatsRuntime(
                $"{Proxy.Id}",
                proxy,
                statsDefault.GetBaseStats(),
                _equipmentService);

            // #4 восстановление голода/жажды из сохранения
            Status = new();
            RestoreSurvivalStatus(consumptionConfig);

            
            // #5 weapon runtime
            Weapon = new UnitWeaponRuntime();
            
            // #6 EFFECTS SYSTEM
            Effects = new ActiveEffectsRuntime();
            Cooldowns = new CooldownTracker();
            
            
            // #7 definition
            var playerCfg = ServiceLocator.Current
                .Get<ConfigProvider>()
                .Get<PlayerArchetypeConfig>();
            Definition = new SurvivorGameplayDefinition(
                new PerceptionDefinition(
                    playerCfg.Perception.detectionRadius,
                    playerCfg.Perception.updateInterval,
                    playerCfg.Perception.hearingRadius,
                    playerCfg.Perception.hearingSensitivity),

                new MeleeCombatDefinition(
                    playerCfg.Combat.AttackRange,
                    playerCfg.Combat.HitRange,
                    playerCfg.Combat.Damage,
                    playerCfg.Combat.Cooldown,
                    playerCfg.Combat.ReadyToAttackAngle),

                new PlayerBrainDefinition(
                    playerCfg.Brain.autoEngageRange,
                    playerCfg.Brain.autoCoverRange,
                    playerCfg.Brain.reEngageDelay),
                
                playerCfg.VoiceAudio
            );
        }
        
        
        /// <summary>
        /// Восстанавливает Hunger/Thirst из Proxy при загрузке и держит их
        /// синхронизированными в дальнейшем (источник изменений — SurvivorDailyConsumptionService,
        /// вызывающий Status.SetHungry/SetThirsty).
        /// </summary>
        private void RestoreSurvivalStatus(SurvivorConsumptionConfig config)
        {
            // #1 применяем сохранённое состояние (баф + флаг), без повторной генерации событий "по кругу"
            if (_stats.GetCurrent(StatId.Hunger) <= 0)
            {
                Status.SetHungry(true);
                _stats.AddBuff(config.HungerBuff);
            }

            if (_stats.GetCurrent(StatId.Thirst) <= 0)
            {
                Status.SetThirsty(true);
                _stats.AddBuff(config.ThirstBuff);
            }

            // #2 держим Proxy в курсе любых будущих изменений статуса
            // (единственный источник изменений — SurvivorDailyConsumptionService)
            Status.HungerChanged += isHungry => Stats.SetStat(StatId.Hunger, isHungry ? 0 : 100);
            Status.ThirstChanged += isThirsty => Stats.SetStat(StatId.Thirst, isThirsty ? 0 : 100);
        }


        /// <summary>
        /// Подписка для обновления модели при поломке снаряги
        /// </summary>
        public void BindInventoryPreview(InventoryAccessService accessService)
        {
            _equipmentService.OnPreviewUpdate += accessService.PreviewUpdated;
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() =>
                _equipmentService.OnPreviewUpdate -= accessService.PreviewUpdated));
        }
        
        
        public void Tick(float dt)
        {
            Effects.Tick(dt);
            Cooldowns.Tick(dt);
        }

        
        #region Invetory Equipment

        public bool Equip(int slotIndex)
        {
            // EquipmentService.HandleSlotChanged(slotIndex, slot);
            // Stats.Recalculate();
            return _equipmentService.Equip(slotIndex);
        }

        public void Unequip(int slotIndex)
        {
            _equipmentService.Unequip(slotIndex);
        }
        
        #endregion


        public EquipmentRuntimeService_Preview GetEquipmentService_Preview()
        {
            var equipment = new EquipmentRuntimeService_Preview();
            equipment.BindSource(_sources[0]);

            return equipment;
        }
    }
}
using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Inventory.Sources;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Meta.Configs.Recruitment;
using UnityEngine;

namespace Galactic1.Code.Systems.Raid.Survivors
{
    /// <summary>
    /// RaidUnitRuntime
    /// ------------------------------------------------------------
    /// Временный runtime юнита в рейде.
    /// Создаётся как snapshot из UnitRuntime.
    /// </summary>
    public sealed partial class RaidUnitRuntime : 
        IEquipmentStateListener,
        IUnitRuntime
    {

        StatsRuntimeBase _stats { get; }
        RaidUnitInventoryRuntime _inventorySource { get; }
        EquipmentRuntimeService _equipmentService { get; }
        //private ItemBrokenHandler _itemBrokenHandler;


        // === IUnitRuntime ===
        public string Id { get; }
        public string ArchetypeId { get; }
        public string DisplayName { get; }
        public int TeamId => 0;
        public bool IsCampDefender { get; }
        public IUnitStatsRuntime Stats => _stats;
        public IUnitInventoryRuntime InventorySource => _inventorySource;
        public IEquipmentStatsProvider EquipmentService => _equipmentService;
        public SurvivorGameplayDefinition Definition { get; }
        public UnitGameplayDefinition RuntimeDefinition => Definition;
        public RaidSurvivorSnapshot Snapshot { get; }

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
        /// Создание рейдового runtime-юнита из стратегического.
        /// </summary>
        public RaidUnitRuntime(
            RaidSurvivorSnapshot snapshot, 
            SurvivorConsumptionConfig consumptionConfig,
            bool campDefeder)
        {
            Id = snapshot.UnitId;
            ArchetypeId = snapshot.ArchetypeId;
            DisplayName = snapshot.DisplayName;
            Snapshot = snapshot;
            Definition = snapshot.GameplayDefinition;
            IsCampDefender = campDefeder;

            // 1️⃣ Инвентарь — ограниченная копия
            _inventorySource = new RaidUnitInventoryRuntime(
                CreateEquipmentInventory(snapshot.InventoryData, snapshot.EquipmentSnapshot),
                null);
            QuickSlot = new QuickSlotMapping(InventorySource.Equipment);

            // 2️⃣ Экипировка — восстановление состояния
            _equipmentService = new EquipmentRuntimeService();

            // 🔹 биндим рейдовый equipment source
            _equipmentService.BindSource(_inventorySource.Equipment);

            // 🔹 берём snapshot из meta-юнита
            //var equipmentSnapshot = runtime.EquipmentService.CreateReadonlySnapshot();

            // 🔹 восстанавливаем экипировку
            _equipmentService.RestoreFromSnapshot(snapshot.EquipmentStateSnapshot);
            
            
            // 3️⃣ Статы — SNAPSHOT
            _stats = new RaidSurvivorStatsRuntime(
                $"{Id}",
                new Dictionary<StatId, float>(
                    snapshot.StatsSnapshot.BaseStats),
                new Dictionary<StatId, float>(
                    snapshot.StatsSnapshot.CurrentStats),
                _equipmentService);

            // #4
            Status = new();
            RestoreSurvivalStatus(snapshot, consumptionConfig);

            // #5 Weapon
            Weapon = new UnitWeaponRuntime();
            
            // #6 EFFECTS SYSTEM
            Effects = new ActiveEffectsRuntime();
            Cooldowns = new CooldownTracker();
        }
        
        
        private void RestoreSurvivalStatus(RaidSurvivorSnapshot snapshot, SurvivorConsumptionConfig config)
        {
            // применяем текущее состояние (баф + флаг)
            if (snapshot.IsHungry)
            {
                Status.SetHungry(true);
                _stats.AddBuff(config.HungerBuff);
            }

            if (snapshot.IsThirsty)
            {
                Status.SetThirsty(true);
                _stats.AddBuff(config.ThirstBuff);
            }
        }


        private IInventorySource CreateEquipmentInventory(UnitRuntime playerUnit, InventoryAccessService access)
        {
            return new RaidUnitInventorySource(
                Id,
                this,
                InventorySnapshot.CreateFromSource(playerUnit.Sources[0], access),
                playerUnit.Sources[0].InventoryData,
                this
            );
        }

        private IInventorySource CreateEquipmentInventory(
            InventoryDataBase inventoryData,
            InventorySnapshot snapshot)
        {
            var runtimeSlots = new List<InventorySlotRuntime>();

            foreach (var slot in snapshot.Slots)
            {
                runtimeSlots.Add(new InventorySlotRuntime(
                    slot.Item,
                    slot.Amount,
                    slot.Durability,
                    slot.AmmoInMagazine));
            }

            return new RaidUnitInventorySource(
                Id,
                this,
                snapshot,
                inventoryData,
                this);
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
            equipment.BindSource(_inventorySource.Equipment);

            return equipment;
        }
    }
}
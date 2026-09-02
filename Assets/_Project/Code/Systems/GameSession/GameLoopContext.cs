
using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Core;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.GameLoop.Tactical;
using Galactic1.Code.Systems.Lifecycle;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Survivors;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Code.UI.Units;
using Galactic1.Configs;
using Galactic1.Core;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Structs;
using UnityEngine;

namespace Galactic1.Code.Systems.GameLoop
{
    /// <summary>
    /// Центральный runtime-контекст кор-лупа.
    /// Является реестром всех runtime-сущностей стратегического слоя.
    ///
    /// ВАЖНО:
    /// - UnitRuntime существует ТОЛЬКО здесь (единственный источник истины)
    /// - Все "составы" (отряды, выбранные на рейд и т.п.) задаются через membership по ID
    /// - Предотвращает рассинхронизацию ссылок между списками
    /// </summary>
    public sealed class GameLoopContext : IUpdate
    {
        
        private readonly GameLoopStateMachine _stateMachine;
        public UnitSceneLifecycleSystem unitSceneLifecycleSystem;
        public TransportSceneLifecycleSystem transportSceneLifecycleSystem;
        
        
        /// <summary>
        /// Ссылка на сериализуемый прокси (meta-слой, сохранения)
        /// </summary>
        public GameLoopContextProxy Proxy { get; private set; }

        public bool IsCampState => !IsWorldMapState && !IsRaidState;
        public bool IsWorldMapState => _stateMachine.IsWorldMapState();
        public bool IsRaidState => _stateMachine.IsRaidState();
        
        
        
        
        
        
        // =========================
        // GLOBAL INBOX SERVICE
        // =========================
        
        public InboxRuntime InboxRuntime { get; private set; }
        
        
        
        // =========================
        // UNIT REGISTRY (SOURCE OF TRUTH)
        // =========================

        public event Action<UnitDisplayData> OnUnitCreated;
        public event Action<string> OnUnitDeleted;          // любое удаление
        public event Action<string> OnUnitDeletedByPlayer;  // только ручное
        public event Action OnUnitChanged;
        
        /// <summary>
        /// Все runtime-юниты игрока.
        /// Ключ — уникальный ID юнита (из Proxy).
        /// </summary>
        private readonly Dictionary<string, UnitRuntime> mapUnits = new();

        /// <summary>
        /// Перечень всех юнитов (readonly доступ для систем)
        /// </summary>
        public IReadOnlyCollection<UnitRuntime> PlayerUnits => mapUnits.Values;

        /// <summary>
        /// Быстрый доступ к юниту по ID
        /// </summary>
        public UnitRuntime GetUnit(string unitId)
        {
            if(mapUnits.TryGetValue(unitId, out var runtime))
                return runtime;
            return null;
        }

    
        /// <summary>
        /// Быстрый доступ к юниту (UnitRuntime/RaidUnitRuntime)
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        public IUnitRuntime GetUnitRuntime(string unitId)
        {
            return !IsRaidState
                ? GetUnit(unitId)
                : CurrentRaid.Squad.GetUnit(unitId);
        }



        private Dictionary<string, UnitDisplayData> mapUnitUI = new();
        
        

        public bool IsStrategicSquadMember(string unitId)
            => _strategicSquadId.Contains(unitId);

        public bool IsCampUnit(string unitId)
            => !_strategicSquadId.Contains(unitId);
        

        // =========================
        // VEHICLES
        // =========================

        private TransportRuntime playerTransport;
        public TransportRuntime PlayerTransport => playerTransport;
        public event Action<TransportRuntime> OnPlayerTransportChanged;

        
        
        // =========================
        // UNIT REGISTRY (SOURCE OF TRUTH)
        // =========================

        // Runtime версии верстаков и производственных объектов
        private readonly Dictionary<string, BaseCampFacilityRuntime> mapFacilities = new();
        

        /// <summary>
        /// Перечень всех зданий (readonly доступ для систем)
        /// </summary>
        public IReadOnlyCollection<BaseCampFacilityRuntime> Facilities => mapFacilities.Values;

        public IEnumerable<CombatFacilityRuntime> DefenseFacilities
        {
            get
            {
                foreach (var facility in mapFacilities.Values)
                {
                    if (facility is CombatFacilityRuntime combat)
                        yield return combat;
                }
            }
        }



        public event Action OnBuildingChanged;
        public event Action<BaseCampFacilityRuntime> OnBuildingCreated;
        public event Action<string> OnBuildingDeleted;

        // =========================
        // CAMP
        // =========================

       
        /// <summary>
        /// Runtime лагеря (экономика, здания и т.п.)
        /// </summary>
        public CampRuntime CampRuntime { get; }
        
        
        
        // =========================
        // MEMBERSHIP (STATE, NOT OBJECTS)
        // =========================
        
        public IReadOnlyList<UnitRuntime> CampUnits =>
            mapUnits.Values
                .Where(x => !_strategicSquadId.Contains(x.Id))
                .ToList();

        /// <summary>
        /// Юниты, входящие в стратегический отряд (карта мира)
        /// </summary>
        private readonly HashSet<string> _strategicSquadId = new();

        /// <summary>
        /// Юниты, выбранные для текущего рейда
        /// </summary>
        private readonly HashSet<string> _tacticalSelectedId = new();


        public List<string> StrategicSquadId => _strategicSquadId.ToList();

        public IEnumerable<UnitRuntime> StrategicSquadUnits
        {
            get
            {
                foreach (var id in _strategicSquadId)
                    yield return mapUnits[id];
            }
        }

        public List<string> TacticalSelectedId => _tacticalSelectedId.ToList();
        public IEnumerable<UnitRuntime> TacticalSelectedUnits
        {
            get
            {
                foreach (var id in _tacticalSelectedId)
                    yield return mapUnits[id];
            }
        }
        
        // =========================
        // STATE MACHINES
        // =========================

        /// <summary>
        /// Родительская state machine кор-лупа
        /// </summary>
        public GameLoopStateMachine GameLoopStateMachine;

        /// <summary>
        /// Тактическая sub-state machine (живет только во время рейда)
        /// </summary>
        public TacticalSubStateMachine TacticalStateMachine;

        /// <summary>
        /// Активный рейд (runtime-песочница)
        /// </summary>
        public RaidRuntime CurrentRaid;

        

        
        // =========================
        // CONSTRUCTOR
        // =========================
        public GameLoopContext(
            GameLoopContextProxy proxy, 
            GameLoopStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            Proxy = proxy;
            
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);

            var configProvider = ServiceLocator.Current.Get<ConfigProvider>();
            var timeService = ServiceLocator.Current.Get<GameTimeService>();
            
            // --- Inbox ---
            InboxRuntime = new InboxRuntime(proxy.BaseProxy, timeService);
            
            
            // --- Units ---
            
            // #1 очищаем от мертвых
            var deadUnits = Proxy.PlayerUnitData.Where(u => u.IsDead.Value).ToList();

            foreach (var dead in deadUnits)
            {
                Proxy.PlayerUnitData.Remove(dead);
                if (Proxy.SquadUnitId.Contains(dead.Id))
                {
                    Proxy.SquadUnitId.Remove(dead.Id);
                    _strategicSquadId.Remove(dead.Id);
                    _tacticalSelectedId.Remove(dead.Id);
                }
            }

            // #2 потом создаем рантайм
            foreach (var unitProxy in proxy.PlayerUnitData)
            {
                var runtime = new UnitRuntime(unitProxy);
                mapUnits.Add(unitProxy.Id, runtime);
                
                // 1️⃣ UI DTO
                var displayData = new UnitDisplayData(runtime);
                RegisterDisplayUnit(displayData);
            }

            // --- Vehicles ---
            playerTransport = new TransportRuntime(proxy.PlayerTransport);
            
            // --- Camp ---
            CampRuntime = new CampRuntime(
                proxy.BaseProxy,
                configProvider,
                timeService);
            
            
            // --- Squad ---
            foreach (var unitId in Proxy.SquadUnitId)
            {
                if (mapUnits.TryGetValue(unitId, out var runtime))
                    SelectForStrategicSquad(unitId);
            }
            
        }


        public void IUpdateClear() {}

        // "глобальный Tick"
        public void UpdateM()
        {
            var dt = Time.deltaTime;
            if (IsRaidState)
            {
                CurrentRaid?.Tick(dt);
                return;
            }

            foreach (var unit in mapUnits.Values)
                unit.Tick(dt);
        }

        
        
        
        // =========================
        // MEMBERSHIP MANAGEMENT
        // =========================
        public void SelectForStrategicSquad(string unitId) => _strategicSquadId.Add(unitId);
        public void DeselectFromStrategicSquad(string unitId) => _strategicSquadId.Remove(unitId);

        public void SelectForTactical(string unitId) => _tacticalSelectedId.Add(unitId);
        public void DeselectFromTactical(string unitId) => _tacticalSelectedId.Remove(unitId);
        public void ClearFromTactical() => _tacticalSelectedId.Clear();



        #region UNIT RUNTIME
        

        /// <summary>
        /// Полное создание нового выжевшего
        /// </summary>
        public void CreateUnitCompletely(
            PlayerProxy newProxy, 
            Dictionary<EquipSlotType, InventorySlotData> inventory = null)
        {
            Proxy.PlayerUnitData.Add(newProxy);
            
            // runtime
            var newRuntime = new UnitRuntime(newProxy);
            mapUnits.Add(newProxy.Id, newRuntime);
            
            // 1️⃣ UI DTO
            var displayData = new UnitDisplayData(newRuntime);
            RegisterDisplayUnit(displayData);

            // добавляем предметы в слоты юнита
            if(inventory != null)
            {
                foreach (var inv in inventory)
                {
                    var slotIndex = EquipmentUtility.GetSlotType(newRuntime.Sources[0], inv.Key);
                    if (slotIndex.HasValue)
                        newRuntime.Sources[0].SetSlot(slotIndex.Value,
                            new InventorySlotRuntime(
                                inv.Value.Item,
                                inv.Value.Amount,
                                inv.Value.Durability,
                                inv.Value.AmmoInMagazine));
                }
            }
            
            // scene instance
            unitSceneLifecycleSystem.HandleUnitCreated(newRuntime);
            OnUnitCreated?.Invoke(mapUnitUI[newProxy.Id]);
            OnUnitChanged?.Invoke();
            
            ServiceLocator.Current.Get<IGameStateProvider>().SaveGameState();
        }
        
        /// <summary>
        /// Полное удаление юнита из игры (с очисткой всех membership)
        /// </summary>
        public bool DeleteUnitCompletely(string unitId)
        {
            if (!mapUnits.Remove(unitId))
                return false;

            var unitProxy = Proxy.PlayerUnitData.FirstOrDefault(u => u.Id == unitId);
            if (unitProxy != null)
            {
                // Удаляем портрет перед удалением юнита (пока не удаляю что бы не было багов)
                //ServiceLocator.Current.Get<CharacterPortraitCache>().Remove(unitProxy.ArchetypeId);
                Proxy.PlayerUnitData.Remove(unitProxy);
            }
            
            // всегда чистим все membership, независимо от состояния proxy
            Proxy.SquadUnitId.Remove(unitId);
            _strategicSquadId.Remove(unitId);
            _tacticalSelectedId.Remove(unitId);
                
            UnregisterDisplayUnit(unitId);
            unitSceneLifecycleSystem.HandleUnitDeleted(unitId);
            OnUnitDeleted?.Invoke(unitId);
            OnUnitChanged?.Invoke();
            ServiceLocator.Current.Get<IGameStateProvider>().SaveGameState();
            return true;
        }
        
        /// <summary>
        /// Полное удаление юнита (игрок сам удаляет)
        /// <br/>(Есть привязка к UI)
        /// </summary>
        public void DeleteUnitByPlayer(string unitId)
        {
            if (DeleteUnitCompletely(unitId))
            {
                OnUnitDeletedByPlayer?.Invoke(unitId);
            }
        }
        
        
        /// <summary>
        /// Очистка от мертвых после рейда
        /// </summary>
        public void CleanupDeadUnitsAfterRaid()
        {
            var revive = DeveloperConsole.I.game.player_revive;
            var toRemove = new List<string>();

            foreach (var unit in mapUnits)
            {
                //DLog.Alert($"Check clean dead: {unit.Value.DisplayName} [{unit.Value.Stats.IsDead}]");
                if (unit.Value.Stats.IsDead)
                {
                    if (revive) // возраждаем мортвых
                        ReviveUnit(unit.Key);
                    else
                        toRemove.Add(unit.Key);
                }
            }

            foreach (var id in toRemove)
                DeleteUnitCompletely(id);
        }
        
        public bool ReviveUnit(string unitId, float hpPercent = 1f)
        {
            // 1. Найти proxy
            var proxy = Proxy.PlayerUnitData.FirstOrDefault(u => u.Id == unitId);
            if (proxy == null)
                return false;

            if (!proxy.IsDead.Value)
                return false;

            // 2. Снять флаг смерти
            proxy.IsDead.Value = false;
            
            var statsBase = ServiceLocator.Current.Get<ConfigProvider>().Get<PlayerStatsBase>();

            // 3. Восстановить HP
            float maxHp = statsBase.BaseStats.health;
            proxy.Stats[StatId.Health].Value = maxHp * hpPercent;

            // 4. Если runtime уже существует — просто обновить
            if (mapUnits.TryGetValue(unitId, out var runtime))
            {
                DLog.Alert($"Revived unit {proxy.Name.Value}", EDlogColor.BLUE);
                runtime.Stats.Revive(proxy.Stats[StatId.Health].Value);
                OnUnitChanged?.Invoke();
                return true;
            }

            Debug.LogError("Revive unit: runtime no exist "+proxy.Name.Value);
            // 5. Если runtime НЕ существует (юнит был удалён) — пересоздать
            // var newRuntime = new UnitRuntime(proxy);
            // mapUnits.Add(unitId, newRuntime);
            //
            // var displayData = new UnitDisplayData(newRuntime);
            // RegisterDisplayUnit(displayData);
            //
            // unitSceneLifecycleSystem.HandleUnitCreated(newRuntime);
            //
            // OnUnitCreated?.Invoke(displayData);
            // OnUnitChanged?.Invoke();

            return true;
        }
        

        #endregion


        #region UNIT DISPLAY

        public UnitDisplayData GetDisplayUnit(string unitId)
        {
            if (mapUnitUI.TryGetValue(unitId, out var data))
                return data;
            return null;
        }

        public IReadOnlyCollection<UnitDisplayData> GetDisplayAllUnit() 
            => mapUnitUI.Values;
        
        public IReadOnlyList<UnitDisplayData> GetDisplaySquadUnit()
        {
            var ar = new List<UnitDisplayData>();
            foreach (var id in _strategicSquadId)
                ar.Add(mapUnitUI[id]);
            
            return ar;
        }

        public void RegisterDisplayUnit(UnitDisplayData unitData) 
            => mapUnitUI.Add(unitData.Id, unitData);

        public void UnregisterDisplayUnit(string unitId)
        {
            if(mapUnitUI.ContainsKey(unitId))
            {
                mapUnitUI[unitId]?.Dispose();
                mapUnitUI.Remove(unitId);
            }
        }
        
        
        // при входе в рейд
        public void RebindDisplayUnitsForRaid(IReadOnlyList<RaidUnitRuntime> raidUnits)
        {
            foreach (var raidUnit in raidUnits)
            {
                if (mapUnitUI.TryGetValue(raidUnit.Id, out var display))
                    display.Rebind(raidUnit);
            }
        }

        // При выходе из рейда — обратно на UnitRuntime
        public void RebindDisplayUnitsAfterRaid()
        {
            foreach (var unit in mapUnits.Values)
            {
                if (mapUnitUI.TryGetValue(unit.Id, out var display))
                    display.Rebind(unit);
            }
        }

        #endregion


        #region TRANSPORT

        public void ReplaceCurrentVehicle(RuntimeId vehicleConfigId)
        {
            if (!GameContent.Items.TryGet(vehicleConfigId, out var vehicleItem))
            {
                Debug.LogError($"Vehicle config not found {vehicleConfigId}");
                return;
            }

            // #1 удалить текущий инстанс
            transportSceneLifecycleSystem.HandleTransportDeleted();

            // #2 обновляем proxy
            Proxy.PlayerTransport.ConfigId.Value = vehicleItem.Id.Guid;

            // #3 обновляем runtime
            playerTransport.ApplyConfig(vehicleItem);
            
            // #4 scene instance
            transportSceneLifecycleSystem.HandleTransportCreated(playerTransport);
            OnPlayerTransportChanged?.Invoke(playerTransport);
            
            ServiceLocator.Current.Get<IGameStateProvider>().SaveGameState();

        }

        #endregion

        #region FACILITY

        // only for load
        public void AddFacility(BaseCampFacilityRuntime runtime)
        {
            if (!mapFacilities.ContainsKey(runtime.Id))
                mapFacilities.Add(runtime.Id, runtime);
        }

        public int GetFacilityCount(FacilityType type)
        {
            int count = 0;

            foreach (var facility in mapFacilities.Values)
                if (facility.Type == type)
                    count++;

            return count;
        }
        public int GetFacilityCount(FacilityModule module)
        {
            if (module?.Item == null)
                return 0;
            
            int count = 0;

            foreach (var facility in mapFacilities.Values)
                if (facility.Config.Item == module.Item)
                    count++;

            return count;
        }
        
        
        /// <summary>
        /// Быстрый доступ к первому зданию указанного runtime-типа.
        /// Используется, когда здание уникально в лагере (например Main Base).
        /// </summary>
        public T GetFacility<T>() where T : BaseCampFacilityRuntime
            => mapFacilities.Values.OfType<T>().FirstOrDefault();
        
        
        /// <summary>
        /// Быстрый доступ к зданию по ID
        /// </summary>
        public bool TryGetBuilding(string id, out BaseCampFacilityRuntime runtime)
            => mapFacilities.TryGetValue(id, out runtime);
        
        public IEnumerable<BaseCampFacilityRuntime> GetByType(FacilityType type)
            => mapFacilities.Values.Where(b => b.Type == type);
        
        /// <summary>
        /// Возвращает первый runtime здания с указанным ConfigId.
        /// Используется для поиска станций по типу (не по уникальному id экземпляра).
        /// </summary>
        public BaseCampFacilityRuntime GetFacilityByConfigId(RuntimeId id)
        {
            foreach (var runtime in mapFacilities.Values)
                if (runtime.Config.Item.Id == id)
                    return runtime;

            return null;
        }

        /// <summary>
        /// Возвращает все runtime зданий с указанным ConfigId.
        /// Нужно если одновременно может быть построено несколько зданий одного типа.
        /// </summary>
        public IEnumerable<BaseCampFacilityRuntime> GetAllFacilitiesByConfigId(RuntimeId id)
        {
            foreach (var runtime in mapFacilities.Values)
                if (runtime.Config.Item.Id == id)
                    yield return runtime;
        }
        

        public BaseCampFacilityRuntime CreateFacilityCompletely(
            FacilityModule buildItem,
            BuildingFootprintRuntime footprint)
            => ServiceLocator.Current.Get<IFacilityRuntimeService>()
                .CreateBuildingCompletely(buildItem, footprint);

        public void DeleteFacilityCompletely(string buildingId)
            => ServiceLocator.Current.Get<IFacilityRuntimeService>().DeleteBuildingCompletely(buildingId);

        public void RegisterFacility(BaseCampFacilityRuntime runtime)
        {
            mapFacilities.Add(runtime.Id, runtime);
            OnBuildingChanged?.Invoke();
            OnBuildingCreated?.Invoke(runtime);
        }

        public void UnregisterFacility(string facilityId)
        {
            mapFacilities.Remove(facilityId);
            OnBuildingChanged?.Invoke();
            OnBuildingDeleted?.Invoke(facilityId);
        }
        
        
        #endregion
    }

}
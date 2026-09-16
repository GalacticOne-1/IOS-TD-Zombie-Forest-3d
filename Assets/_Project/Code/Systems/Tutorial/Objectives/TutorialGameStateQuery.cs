using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Presentation;
using Galactic1.Code.Systems.Tutorial.Runtime;
using Galactic1.Code.UI.Inventory;
using Galactic1.Configs.Galactic1.Code.GameDatabase;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Единственная реализация пяти узких интерфейсов (IGameLoopStateQuery,
    /// ITutorialInventoryQuery, ITutorialSquadQuery, ITutorialUIStateQuery,
    /// ITutorialConstructionQuery) — один класс резолвит GameLoopContext/
    /// GameLoopStateMachine/UIScreenManager через конструктор и ленивый ServiceLocator
    /// lookup, но каждый объектив/guidance condition принимает только тот интерфейс,
    /// который ему реально нужен (не God-интерфейс).
    /// </summary>
    public sealed class TutorialGameStateQuery :
        IGameLoopStateQuery, 
        ITutorialInventoryQuery, 
        ITutorialInboxQuery,
        ITutorialSquadQuery, 
        ITutorialUIStateQuery,
        ITutorialFacilityPanelQuery,
        ITutorialInventoryInteractionQuery,
        ITutorialConstructionQuery
    {
        private readonly GameLoopContext _context;
        private readonly GameLoopStateMachine _stateMachine;
        private UIScreenManager _uiScreenManager;
        
        private readonly TutorialInventoryViewRegistry _inventoryViews;

        public event Action<TutorialStepDomain, TutorialStepDomain> OnDomainTransition;

        private TutorialStepDomain _lastDomain;
        private FacilityType? _openFacilityType;
        
        

        public TutorialGameStateQuery(
            GameLoopContext context, 
            GameLoopStateMachine stateMachine, 
            TutorialInventoryViewRegistry inventoryViews)
        {
            _context = context;
            _stateMachine = stateMachine;
            _inventoryViews = inventoryViews;
            
            
            // Fix: source of truth — сам факт "какая панель сейчас открыта", а не флаг,
            // управляемый вручную снаружи. Событие только обновляет это состояние, читается
            // оно всегда через IsFacilityPanelOpen(), не наоборот (тот же принцип, что у
            // остальных query этого класса).
            EventBus<FacilityPanelOpenedEvent>.Register(new EventBinding<FacilityPanelOpenedEvent>(
                e => _openFacilityType = e.Type));
            EventBus<UIScreenClosedEvent>.Register(new EventBinding<UIScreenClosedEvent>(e =>
            {
                if (e.ScreenId == UIScreenId.FacilityPanel)
                    _openFacilityType = null;
            }));

            // === ITutorialConstructionQuery: OnBuildingCreated — плоский C#-event
            // GameLoopContext, тот же приём, что уже используется для OnUnitCreated в
            // RecruitCompletedObjective. Оборачиваем в Action<ItemId>, чтобы guidance
            // condition не знал про BaseCampFacilityRuntime.
            _context.OnBuildingCreated += runtime => OnFacilityBuilt?.Invoke((ItemId)runtime.Config.Item.Id);

            // === активация по одноразовому событию старта игры ===
            EventBus<StartGameEvent>.Register(new EventBinding<StartGameEvent>(() =>
            {
                _lastDomain = ComputeDomain();
                _stateMachine.OnStateChanged += _ =>
                {
                    var newDomain = ComputeDomain();
                    if (newDomain != _lastDomain)
                    {
                        var previous = _lastDomain;
                        _lastDomain = newDomain;
                        OnDomainTransition?.Invoke(previous, newDomain);
                    }
                };


                /*
                 * Sart() or Restore() должен вызываться после того
                 * как gameSession.GameLoopContext уже полностью восстановлен из сейва
                 * (юниты/здания/CampRuntime загружены)
                 */
                ServiceLocator.Current.Get<ITutorialService>().StartOrRestore(GameIdProvider.TutorialStart);
            }));
        }

        private TutorialStepDomain ComputeDomain()
        {
            if (_context.IsRaidState) return TutorialStepDomain.Raid;
            if (_context.IsWorldMapState) return TutorialStepDomain.WorldMap;
            return TutorialStepDomain.Camp; // см. известное ограничение re: PostRaidReport
        }

        public TutorialStepDomain CurrentDomain => ComputeDomain();

        // ── ITutorialInventoryQuery ──────────────────────────────────────
        public bool IsItemEquippedByAnyStrategicUnit(EquipSlotType slot, ItemId itemId = null)
        {
            foreach (var unit in _context.StrategicSquadUnits)
            {
                var source = unit.InventorySource?.Equipment;
                if (source == null) continue;
                var slots = source.GetSlots();
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].IsEmpty) continue;
                    if (source.GetEquipmentSlotType(i) != slot) continue;
                    if (itemId != null && slots[i].Item.Id != itemId) continue;
                    return true;
                }
            }
            return false;
        }

        public int GetCampStorageAmount(ItemId itemId)
        {
            var source = _context.CampRuntime.GetInventory(StorageType.Regular);
            return source?.GetTotalAmount(itemId) ?? 0;
        }
        
        
        
        // ── ITutorialInboxQuery ───────────────────────────────────────────
        public bool HasItemInInbox(ItemId itemId)
            => _context.InboxRuntime.GetTotalAmount(itemId) > 0;

        
        public event Action OnInboxChanged
        {
            add => _context.InboxRuntime.OnInboxChanged += value;
            remove => _context.InboxRuntime.OnInboxChanged -= value;
        }
        
            

        

        // ── ITutorialSquadQuery ───────────────────────────────────────────
        public int GetStrategicSquadSize() => _context.StrategicSquadId.Count;

        public bool SurvivorIsFree()
        {
            var unitId = ServiceLocator.Current.Get<InventoryManagementWindow>()
                .modeController.SelectedUnit.unitId;
            if (string.IsNullOrEmpty(unitId))
                return false;
            
            return !_context.IsStrategicSquadMember(unitId);
        }

        // ── ITutorialUIStateQuery ─────────────────────────────────────────
        // Требует UIScreenManager.IsScreenOpen(UIScreenId) — если такого метода сегодня нет,
        // это одна интеграционная точка той же природы, что уже существующие "требует одну
        // строку в X" объективы (см. UIScreenOpenedObjective/ButtonPressedObjective), а не
        // архитектурная переделка UI-системы. Пока метод не добавлен, guidance-условия на
        // основе UIScreenOpenGuidanceConditionDefinition будут всегда считать экран закрытым
        // (IsScreenOpen возвращает то, что вернёт заглушка/исключение UIScreenManager —
        // явно проверьте это перед использованием этого condition в продакшн-контенте).
        public bool IsScreenOpen(UIScreenId screenId)
        {
            if (screenId == null) return false;
            _uiScreenManager ??= ServiceLocator.Current.Get<UIManager>().ScreenManager;
            // var result = _uiScreenManager.IsScreenOpen(screenId);
            // Debug.Log($"[Tutorial] IsScreenOpen({screenId}) = {result}");
            return _uiScreenManager.IsScreenOpen(screenId);
        }
        
        public bool IsFacilityPanelOpen(FacilityType type) 
            => _openFacilityType == type;
        
        public bool IsItemSelected(ItemId itemId)
        {
            foreach (var view in _inventoryViews.Active)
            {
                var selected = view.selectedSlot;
                if (selected == null) continue;
                var slot = view.GetSlot(selected.SlotIndex);
                if (!slot.IsEmpty && (itemId == null || slot.Item.Id == itemId))
                    return true;
            }
            return false;
        }
        
        // Намеренно БЕЗ кэширования InventoryManagementWindow (в отличие от _uiScreenManager
        // выше): это screen-инстанс, а не persistent-менеджер — пересоздаётся при каждом
        // открытии/закрытии инвентаря, кэш через ??= рисковал бы Unity fake-null после
        // Destroy(). Резолвим заново на каждый вызов.
        public bool IsItemBeingDragged(ItemId itemId)
        {
            var window = ServiceLocator.Current.Get<InventoryManagementWindow>();
            var dragged = window?.Drag?.DraggedItemId;
            return dragged != null && (itemId == null || dragged == itemId);
        }

        // ── ITutorialConstructionQuery ────────────────────────────────────
        public event Action<ItemId> OnFacilityBuilt;

        public bool HasFacilityBuilt(ItemId itemId)
        {
            foreach (var facility in _context.Facilities)
                if (itemId == null || facility.Config.Item.Id == itemId)
                    return true;
            return false;
        }
    }
}
using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Runtime;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Единственная реализация трёх узких интерфейсов (IGameLoopStateQuery,
    /// ITutorialInventoryQuery, ITutorialSquadQuery) — один класс резолвит
    /// GameLoopContext/GameLoopStateMachine через конструктор, но каждый объектив
    /// принимает только тот интерфейс, который ему реально нужен (не God-интерфейс).
    /// </summary>
    public sealed class TutorialGameStateQuery :
        IGameLoopStateQuery, ITutorialInventoryQuery, ITutorialSquadQuery
    {
        private readonly GameLoopContext _context;
        private readonly GameLoopStateMachine _stateMachine;

        public event Action<TutorialStepDomain, TutorialStepDomain> OnDomainTransition;

        private TutorialStepDomain _lastDomain;

        public TutorialGameStateQuery(GameLoopContext context, GameLoopStateMachine stateMachine)
        {
            _context = context;
            _stateMachine = stateMachine;
            
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
                 * Restore() должен вызываться после того
                 * как gameSession.GameLoopContext уже полностью восстановлен из сейва
                 * (юниты/здания/CampRuntime загружены)
                 */
                ServiceLocator.Current.Get<ITutorialService>().Restore();
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

        // ── ITutorialSquadQuery ───────────────────────────────────────────
        public int GetStrategicSquadSize() => _context.StrategicSquadId.Count;
    }
}


using System.Linq;
using Galactic1.Code.Systems.World.Threats;
using Galactic1.Game.Camp.Proxy;
using Galactic1.Structs;
using ObservableCollections;
using R3;
using UnityEngine;

namespace Galactic1.Code.Core
{
    /// <summary>
    /// Сериализуемое состояние GameLoopContext.
    /// Не содержит runtime-ссылок.
    public class GameLoopContextProxy
    {
        public readonly GameLoopContextData Origin;



        // --- META ---
        public readonly ReactiveProperty<int> CurrentLocationStateId;
        public readonly ReactiveProperty<bool> PlayerOnMap;
        public readonly ReactiveProperty<string> CurrentLocationNode;
        public readonly ReactiveProperty<int> CurrentDay;
        public readonly ReactiveProperty<int> RemainingHour;
        public readonly ReactiveProperty<ThreatSaveData> ThreatData;

        // --- FLOW ---
        public readonly ReactiveProperty<int> CurrentState;

        // --- RAID ---
        public RaidResultProxy LastRaidResult;
        public readonly ReactiveProperty<bool> HasPendingRaidReport;


        // --- ETNITIES ---
        public ObservableList<PlayerProxy> PlayerUnitData { get; } = new();
        public ObservableList<string> SquadUnitId { get; } = new();
        public TransportProxy PlayerTransport;
        public BaseProxy BaseProxy;
        
        
        // --- CARGO DRONE ---
        public ReactiveProperty<int> RemainingDroneCharge;
        


        // --- Флаг, активированы ли подписки LiveSync ---
        private bool _liveSyncActive;

        public GameLoopContextProxy(GameLoopContextData data)
        {
            Origin = data;

            // --- Load Phase ---

            // === вариант локации для загрузки сцены (-1/ 0/ N) 
            CurrentLocationStateId = new(Origin.CurrentLocationStateId);
            CurrentLocationStateId.Skip(1).Subscribe(_ => Origin.CurrentLocationStateId = _);

            // конкретная локация 
            CurrentLocationNode = new(Origin.CurrentLocationNode);
            CurrentLocationNode.Subscribe(_ => Origin.CurrentLocationNode = _);

            PlayerOnMap = new(Origin.PlayerOnMap);
            PlayerOnMap.Subscribe(_ => Origin.PlayerOnMap = _);

            CurrentDay = new(Origin.CurrentDay);
            RemainingHour = new(Origin.RemainingHour);
            ThreatData = new(Origin.ThreatData);
            CurrentState = new(Origin.CurrentState);
            LastRaidResult = new(Origin.LastRaidResult);
            //HasPendingBaseReport = new(Origin.HasPendingBaseReport);
            HasPendingRaidReport = new(Origin.HasPendingRaidReport);

            BaseProxy = new BaseProxy(Origin.BaseData);


            RemainingDroneCharge = new(Origin.RemainingDroneCharge);


            LoadPlayerUnits();
            LoadPlayerTransport();
        }

        #region Load Phase

        private void LoadPlayerUnits()
        {
            // Загружаем все юниты
            foreach (var unitData in Origin.PlayerUnitData)
                PlayerUnitData.Add(new PlayerProxy(unitData));

            // Восстанавливаем выбранные юниты отряда
            foreach (var id in Origin.SquadUnitId)
            {
                var unit = PlayerUnitData.FirstOrDefault(u => u.Origin.Id == id);
                if (unit != null)
                    SquadUnitId.Add(unit.Id);
            }
        }

        private void LoadPlayerTransport()
        {
            if (Origin.PlayerTransport == null)
                Origin.PlayerTransport = new TransportData();

            PlayerTransport = new TransportProxy(Origin.PlayerTransport);
        }

        #endregion

        #region LiveSync Phase

        /// <summary>
        /// Активирует LiveSync подписки.
        /// Должно вызываться один раз после загрузки данных.
        /// </summary>
        public void ActivateLiveSync()
        {
            if (_liveSyncActive) return;
            _liveSyncActive = true;

            // --- META ---
            CurrentDay.Skip(1).Subscribe(v => Origin.CurrentDay = v);
            RemainingHour.Skip(1).Subscribe(v => Origin.RemainingHour = Mathf.Clamp(v, 0, 24));
            ThreatData.Skip(1).Subscribe(v => Origin.ThreatData = v);
            CurrentState.Skip(1).Subscribe(v => Origin.CurrentState = v);
            //HasPendingBaseReport.Skip(1).Subscribe(v => Origin.HasPendingBaseReport = v);
            HasPendingRaidReport.Skip(1).Subscribe(v => Origin.HasPendingRaidReport = v);

            // --- PlayerUnitData ---
            PlayerUnitData.ObserveAdd().Subscribe(e => Origin.PlayerUnitData.Add(e.Value.Origin));
            PlayerUnitData.ObserveRemove().Subscribe(e =>
            {
                Origin.PlayerUnitData.Remove(Origin.PlayerUnitData.FirstOrDefault(u => u.Id == e.Value.Id));
                SquadUnitId.Remove(e.Value.Id);
            });

            SquadUnitId.ObserveAdd().Subscribe(e => Origin.SquadUnitId.Add(e.Value));
            SquadUnitId.ObserveRemove().Subscribe(e => Origin.SquadUnitId.Remove(e.Value));


            // --- Player Transport ---
            PlayerTransport.ConfigId
                .Skip(1)
                .Subscribe(v => Origin.PlayerTransport.ConfigId = v);
            
            
            // --- Cargo Drone ---
            RemainingDroneCharge.Skip(1).Subscribe(v => Origin.RemainingDroneCharge = v);
        }

        #endregion
    }
}

using System;
using System.Linq;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Manages squad composition on strategic layer (Map).
    /// Units traveling together.
    /// </summary>
    public sealed class StrategicSquadSystem
    {
        private GameLoopContext _gameLoopContext;
        private int maxSquadSize = 6; //  <-- берем из текущей техники
        

        public event Action<string, bool> OnSquadChanged;

        public StrategicSquadSystem(GameLoopContext context)
        {
            _gameLoopContext = context;

            
            RebindSquadSize(_gameLoopContext.PlayerTransport);
            _gameLoopContext.OnPlayerTransportChanged += RebindSquadSize;
            
            

            // не удалять !!! должны быть полная очистка, т.к много подписчиков
            EventBus<SceneServicesResetReusableEvent>.Register(
                new EventBinding<SceneServicesResetReusableEvent>(() => OnSquadChanged = null));
        }

        // =========================================================
        // PUBLIC API
        // =========================================================
        
        /// <summary>
        /// Для обновления размера отряда под текущую машину
        /// </summary>
        /// <param name="transportRuntime"></param>
        private void RebindSquadSize(TransportRuntime transportRuntime)
        {
            maxSquadSize = transportRuntime.Item.Vehicle.SquadSlots;
            
            // срезаем отряд если превышен лимит
            var squadUnits = _gameLoopContext.StrategicSquadUnits;

            if (squadUnits.Count() >= maxSquadSize)
            {
                var _squad = squadUnits.ToArray();
                var current = squadUnits.Count();

                for (int i = current - 1; i >= maxSquadSize; i--)
                {
                    RemoveUnit(_squad[i]);
                }
                
                // todo
                // алерт игроку об уменьшении отряда
            }
        }

        public bool AddUnit(UnitRuntime playerUnit)
        {
            if (playerUnit == null)
                return false;

            var squadUnits = _gameLoopContext.StrategicSquadUnits;

            if (squadUnits.Contains(playerUnit))
                return false;

            if (squadUnits.Count() >= maxSquadSize)
                return false;

            _gameLoopContext.SelectForStrategicSquad(playerUnit.Proxy.Id);
            SyncToProxy();
            OnSquadChanged?.Invoke(playerUnit.Proxy.Id, true);
            EventBus<StrategicSquadChangedEvent>.Raise(new StrategicSquadChangedEvent());
            
            return true;
        }

        public void RemoveUnit(UnitRuntime playerUnit)
        {
            if (playerUnit == null)
                return;

            _gameLoopContext.DeselectFromStrategicSquad(playerUnit.Proxy.Id);
            SyncToProxy();
            OnSquadChanged?.Invoke(playerUnit.Proxy.Id, false);
            EventBus<StrategicSquadChangedEvent>.Raise(new StrategicSquadChangedEvent());
        }

        // public void ClearSquad()
        // {
        //     gameLoop.StrategicSquadUnits.Clear();
        //     SyncToProxy();
        //     OnSquadChanged?.Invoke();
        // }

        public bool IsInSquad(string unitId)
            => _gameLoopContext.StrategicSquadId.Contains(unitId);

        public bool CanAddMoreUnits()
            => _gameLoopContext.StrategicSquadUnits.Count() < maxSquadSize;

        /// <summary>
        /// Вернет текущий отряд N/Max
        /// </summary>
        /// <returns></returns>
        public (int, int) GetSquadStat() 
            => (_gameLoopContext.StrategicSquadUnits.Count(), maxSquadSize);

        // =========================================================
        // PERSISTENCE SYNC
        // =========================================================

        void SyncToProxy()
        {
            var proxyUnitId = _gameLoopContext.Proxy.SquadUnitId;
            var runtimeIds = _gameLoopContext.StrategicSquadUnits.Select(u => u.Proxy.Id).ToList();
            
            // Удаляем лишние
            for (int i = proxyUnitId.Count - 1; i >= 0; i--)
            {
                if (!runtimeIds.Contains(proxyUnitId[i]))
                    proxyUnitId.RemoveAt(i); // ← вызовет ObserveRemove
            }
            
            // Добавляем отсутствующие
            foreach (var id in runtimeIds)
            {
                if (!proxyUnitId.Contains(id))
                    proxyUnitId.Add(id); // ← вызовет ObserveAdd
            }
        }
    }
}

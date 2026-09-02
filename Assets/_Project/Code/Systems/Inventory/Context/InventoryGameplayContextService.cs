
using System.Linq;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Sources;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.Systems.Squad;
using Galactic1.Code.UI.RaidReport;
using Galactic1.Configs;
using Galactic1.Core.Systems.GameLoopSession;

namespace Galactic1.Code.Inventory.Context
{
    public sealed class InventoryGameplayContextService
    {
        private readonly GameSession _session;
        private readonly GameLoopStateMachine _gameLoopStateMachine;
        private readonly StrategicSquadSystem _strategicSquad;


        public InventoryGameplayContextService(
            DIContainer container, 
            GameSession session)
        {
            _session = session;
            _gameLoopStateMachine = container.Resolve<GameLoopStateMachine>();
            _strategicSquad = _session.StrategicSquadSystem;
        }

        public (IInventorySource left, IInventorySource right) BuildMode(InventoryGameplayMode mode)
        {
            var glc = _session.GameLoopContext;

            return mode switch
            {
                InventoryGameplayMode.Camp_AllUnits => BuildCampAllUnits(glc),
                InventoryGameplayMode.Transport_SquadOnly => BuildTransportSquad(glc),
                InventoryGameplayMode.Camp_SquadOnly => BuildCampSquad(glc),
                InventoryGameplayMode.Camp_And_Transport => BuildCampTransport(glc),
                InventoryGameplayMode.Transport_BufferLoot => BuildTransportBufferLoot(glc),
                InventoryGameplayMode.Transport_BufferDrone => BuildTransportBufferDrone(glc),
                _ => default
            };
        }
        
        private (IInventorySource, IInventorySource) BuildCampAllUnits(GameLoopContext glc)
        {
            var left = glc.CampRuntime.Sources[0];
            var right = glc.PlayerUnits.FirstOrDefault()?.Sources[0];

            return (left, right);
        }

        private (IInventorySource, IInventorySource) BuildCampTransport(GameLoopContext glc)
        {
            var left = glc.CampRuntime.Sources[0];
            var right = glc.PlayerTransport.GetInventory;

            return (left, right);
        }
        
        private (IInventorySource, IInventorySource) BuildCampSquad(GameLoopContext glc)
        {
            var left = glc.CampRuntime.Sources[0];
            var right = glc.StrategicSquadUnits.FirstOrDefault()?.Sources[0];

            return (left, right);
        }
        
        private (IInventorySource, IInventorySource) BuildTransportSquad(GameLoopContext glc)
        {
            IInventorySource left, right;

            // === здесь передаем источники во время рейда
            if (_session.GameLoopContext.IsRaidState)
            {
                var raid = glc.CurrentRaid;

                left = raid.PlayerTransport.Sources.Cargo;
                right = raid.Squad.Units.FirstOrDefault()?.InventorySource.Equipment;
                // right = transport.equipment ...

                return (left, right);
            }

            // meta-режим
            left = glc.PlayerTransport.GetInventory;
            right = glc.StrategicSquadUnits.FirstOrDefault()?.Sources[0];
            // right = transport.equipment ...

            return (left, right);
        }
        
        
        private (IInventorySource, IInventorySource) BuildTransportBufferLoot(GameLoopContext glc)
        {
            var left = glc.PlayerTransport.GetInventory;
            var right = ServiceLocator.Current.Get<RaidReportFlowController>().InventoryContext.LootBufferSource;

            return (left, right);
        }
        private (IInventorySource, IInventorySource) BuildTransportBufferDrone(GameLoopContext glc)
        {
            var left = glc.PlayerTransport.GetInventory;
            var right = ServiceLocator.Current.Get<RaidReportFlowController>().InventoryContext.DroneBufferSource;

            return (left, right);
        }

        
        
        
        
        
        public IInventorySource GetUnitInventorySource(UnitRuntime playerUnit, InventoryGameplayMode mode)
        {
            if (playerUnit == null)
                return null;

            if (_session.GameLoopContext.IsRaidState)
            {
                var raidUnit = _session.GameLoopContext.CurrentRaid
                    .Squad.Units
                    .FirstOrDefault(u => u.Id == playerUnit.Id);

                return raidUnit?.InventorySource.Equipment;
            }

            return playerUnit.Sources[0];
        }
        

    }
}
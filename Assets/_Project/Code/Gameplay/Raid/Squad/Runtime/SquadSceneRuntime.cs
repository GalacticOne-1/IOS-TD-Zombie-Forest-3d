using System;
using System.Collections.Generic;
using Galactic1.AbstractFactory;
using Galactic1.Code.Gameplay.Units;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Тактический runtime отряда.
    /// Живёт только в рейде (сцена-слой).
    /// Агрегирует сцен-объекты юнитов и состояние отряда.
    /// </summary>
    public sealed class SquadSceneRuntime
    {
        
        public SquadCommandBuffer Commands { get; } = new();
        
        // ========================= 
        // Members
        // =========================
        private readonly List<SurvivorInstance> _agents = new();
        public IReadOnlyList<SurvivorInstance> Agents => _agents;
        public SurvivorInstance GetLeader => _agents.Count > 1 ? _agents[1] : _agents[0];

        // =========================
        // State
        // =========================
        public SquadState State { get; private set; } = SquadState.Idle;
        
        public event Action CompositionChanged;
        
        


        // =========================
        // API
        // =========================
        public void AddAgent(SurvivorInstance agent)
        {
            _agents.Add(agent);
            agent.OnDeath += OnAgentDeath;
        }
        public void RemoveAgent(SurvivorInstance agent) => _agents.Remove(agent);
        
        
        private void OnAgentDeath(_Object_ agent)
        {
            Commands.Enqueue(() =>
            {
                agent.OnDeath -= OnAgentDeath;
                _agents.Remove((SurvivorInstance)agent);
                
                CompositionChanged?.Invoke();
            });
        }


        public void SetState(SquadState newState)
        {
            if (State == newState) 
                return;
            State = newState;
        }
        
        public Vector3 ComputeMassCenter()
        {
            if (_agents.Count == 0) return Vector3.zero;
            
            Vector3 sum = Vector3.zero;
            foreach (var a in _agents)
                sum += a.transform.position;

            return sum / _agents.Count;
        }
    }
}
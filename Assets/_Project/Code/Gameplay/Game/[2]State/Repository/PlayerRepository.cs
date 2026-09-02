using System.Collections.Generic;
using Galactic1.AbstractFactory;
using Galactic1.Gameplay.Player;
using UnityEngine;

namespace Galactic1.Repository
{
    public class PlayerRepository : Repository, IGameService
    {
        private Dictionary<string, _Entity> _units = new();
        
        public override void Register(string withId, GameObject entity)
            => _units.Add(withId, entity.GetComponent<_Entity>());

        public override void Unregister(string withId, GameObject entity)
            => _units.Remove(withId);

        
        
        
        
        public PlayerController GetController => _units["player"] as PlayerController;
        
        public _Entity GetUnit(string withId) => _units[withId];
        
        public override GameObject GetCloseet()
        {
            throw new System.NotImplementedException();
        }
    }
}
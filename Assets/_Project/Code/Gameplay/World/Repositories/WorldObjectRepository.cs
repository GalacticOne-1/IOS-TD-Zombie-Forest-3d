using System.Collections.Generic;
using Galactic1.Code.Core.Repositories;
using UnityEngine;

namespace Galactic1.Code.Gameplay.World.Repositories
{
    public class WorldObjectRepository : IRepository<WorldInstance>, IGameService
    {
        private readonly Dictionary<string, WorldInstance> objects = new();
        
        public IReadOnlyDictionary<string, WorldInstance> All => objects;
        
        
        public void Register(string withId, WorldInstance instance)
        {
            if (string.IsNullOrEmpty(withId) || instance == null)
                return;

            if (objects.ContainsKey(withId))
            {
                //Debug.LogWarning($"Building with ID {withId} already registered");
                return;
            }

            objects.Add(withId, instance);
        }

        public void Unregister(string withId, WorldInstance instance)
        {
            if (instance == null || string.IsNullOrEmpty(withId))
                return;

            objects.Remove(withId);
        }

        public WorldInstance Get(string id)
        {
            objects.TryGetValue(id, out var instance);
            return instance;
        }


        public void Clear()
        {
            objects.Clear();
        }
    }
}
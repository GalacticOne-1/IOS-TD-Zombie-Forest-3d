using System.Collections.Generic;
using Galactic1.Code.Core.Repositories;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Gameplay.BaseBuilding
{
    /// <summary>
    /// Репозиторий всех зданий лагеря.
    /// </summary>
    public class BaseFacilityRepository : IRepository<FacilityInstance>, IGameService
    {
        private readonly Dictionary<string, FacilityInstance> buildings = new();
        
        public IReadOnlyDictionary<string, FacilityInstance> All => buildings;
        
        
        public void Register(string withId, FacilityInstance instance)
        {
            if (string.IsNullOrEmpty(withId) || instance == null)
                return;

            if (buildings.ContainsKey(withId))
            {
                //Debug.LogWarning($"Building with ID {withId} already registered");
                return;
            }

            buildings.Add(withId, instance);
        }

        public void Unregister(string withId, FacilityInstance instance)
        {
            if (instance == null || string.IsNullOrEmpty(withId))
                return;

            buildings.Remove(withId);
        }
        
        
        public (bool done, FacilityInstance instance) TryGetWithConfig(ItemId configId)
        {
            foreach (var b in buildings.Values)
            {
                if (b.ItemConfig.Id == configId)
                    return (true, b);
            }

            return (false, null);
        }

        public (bool done, FacilityInstance instance) TryGet(string uniqueId)
        {
            return (buildings.TryGetValue(uniqueId, out var instance), instance);
        }


        public void Clear()
        {
            buildings.Clear();
        }

        
    }
}
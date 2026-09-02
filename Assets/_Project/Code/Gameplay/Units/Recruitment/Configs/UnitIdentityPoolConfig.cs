using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.UI.CharacterPreview;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Meta.Configs.Recruitment
{
    /// <summary>
    /// Пул имён и портретов для генерации личности.
    /// </summary>
    [CreateAssetMenu(
        fileName = "UnitIdentityPoolConfig",
        menuName = "Game Configs/Recruitment/Unit Identity Pool Config")]
    public class UnitIdentityPoolConfig : ScriptableObject
    {
        [SerializeField] private NameSet NameSetMale;
        [SerializeField] private NameSet NameSetFemale;
        
        [System.Serializable]
        public struct NameSet
        {
            public List<string> Name;
            public List<string> LastName;
        }
        
        
        [field: SerializeField] public List<Sprite> Portraits { get; private set; }


        [Header("Visual")] 
        [SerializeField] private UIPreviewConfig previewConfig;
        [SerializeField] private List<ArchetypePrefabEntry> survivorVariant = new();

        [System.Serializable]
        public sealed class ArchetypePrefabEntry
        {
            public string Key;
            public AppearanceId AppearanceId;
            public bool Female;

        }

        private string PrefabPath = "survivor_1";

        public UIPreviewConfig PreviewConfig => previewConfig;


        public NameSet GetNameSet(bool female)
            => !female
                ? NameSetMale
                : NameSetFemale;


        /// <summary>
        /// Возвращает случайный архетип которого нет в занятых.
        /// Если все заняты — возвращает любой случайный.
        /// </summary>
        public string GetAvailableArchetype(IReadOnlyCollection<string> usedArchetypeIds)
        {
            // Фильтруем незанятые
            var available = new List<ArchetypePrefabEntry>();
            foreach (var entry in survivorVariant)
            {
                if (entry.Key == null) continue;
                if (!usedArchetypeIds.Contains(entry.Key))
                    available.Add(entry);
            }

            // Если все заняты — берём любой
            var pool = available.Count > 0 ? available : survivorVariant;
            if (pool.Count == 0) return null;

            return pool[Random.Range(0, pool.Count)].Key;
        }



        /// <summary>
        /// Возвращает префаб персонажа по архетипу.
        /// Если не найден — возвращает первый в списке как fallback.
        /// </summary>
        // public string GetPrefab(string key)
        // {
        //     foreach (var entry in prefabs)
        //         if (entry.Key == key)
        //             return entry.PrefabPath;
        //
        //     return prefabs.Count > 0 ? prefabs[0].PrefabPath : null;
        // }
        
        public (string prefabPath, ArchetypePrefabEntry variant) GetSurvivorEntry(string key)
        {
            foreach (var entry in survivorVariant)
                if (entry.Key == key)
                    return (PrefabPath, entry);
            
            return (PrefabPath, survivorVariant[0]);
        }

    }
}
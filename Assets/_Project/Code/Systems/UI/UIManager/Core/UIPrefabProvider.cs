using System.Collections.Generic;
using UnityEngine;
#if ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
#endif

namespace Galactic1.UI.Core
{
    public class UIPrefabProvider : MonoBehaviour
    {
        
        [Header("Configs")] 
        [SerializeField]
        private UIRegistry registry;
        [SerializeField]
        private List<UIPopupConfig> popupConfigs = new();


        private readonly Dictionary<UIScreenId, GameObject> cache = new();
        private readonly Dictionary<UIScreenId, UIPopupConfig> popupMap = new();


        private void Awake()
        {
            foreach (var cfg in popupConfigs)
            {
                if (cfg == null) continue;
                popupMap[cfg.id] = cfg;
                if (cfg.prefab != null && !cache.ContainsKey(cfg.id))
                    cache[cfg.id] = cfg.prefab;
            }
        }


        public UIPopupConfig GetPopupConfig(UIScreenId id)
        {
            popupMap.TryGetValue(id, out var cfg);
            return cfg;
        }


        public GameObject LoadSync(UIScreenId id)
        {
            if (cache.TryGetValue(id, out var p))
                return p;


            var cfg = GetPopupConfig(id);
            if (cfg != null && cfg.prefab != null)
            {
                cache[id] = cfg.prefab;
                return cfg.prefab;
            }


            // Fallback: try Resources
            var path = registry.GetPath(id);
            var res = Resources.Load<GameObject>(path);
            if (res != null)
            {
                cache[id] = res;
                return res;
            }


            Debug.LogError($"[UIPrefabProvider] Could not find prefab for id={id}");
            return null;
        }


#if ADDRESSABLES
public async Task<GameObject> LoadAsync(string id)
{
if (cache.TryGetValue(id, out var p))
return p;


var cfg = GetPopupConfig(id);
string key = cfg != null && !string.IsNullOrEmpty(cfg.addressableKey) ? cfg.addressableKey : id;


var handle = Addressables.LoadAssetAsync<GameObject>(key);
await handle.Task;
if (handle.Status == AsyncOperationStatus.Succeeded)
{
cache[id] = handle.Result;
return handle.Result;
}


Debug.LogWarning($"[UIPrefabProvider] Addressables failed for {id}, falling back to Resources");
return LoadSync(id);
}
#endif


    }
}
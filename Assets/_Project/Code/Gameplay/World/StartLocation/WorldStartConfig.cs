
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Configs;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;
using Galactic1.Utility;
using UnityEngine;

namespace Galactic1.Game.World.StartLocation
{
    /// <summary>
    /// Описывает стартовое состояние локации.
    /// Используется только при NewGame() — один раз за всё время.
    /// Редактируется дизайнерами в инспекторе.
    /// </summary>
    [CreateAssetMenu(fileName = "WorldStartConfig", menuName = "Game Configs/World Start/WorldStartConfig")]
    public sealed class WorldStartConfig : ScriptableObject
    {
        [SerializeField] private FacilityPlacementData[] facilities;

        [Space] [SerializeField] private ItemConfig transport;

        
        
        
        public ItemConfig Transport => transport;


        public IReadOnlyList<FacilityData> StartFacilities(
            ConfigProvider configProvider)
        {
            List<FacilityData> f = new();

            var inventoryConfig = configProvider.Get<CampStorageInventoryConfig>();

            var l = facilities.Length;
            for (int i = 0; i < l; i++)
            {
                var sign = GetId(inventoryConfig, facilities[i].config);
                f.Add(new FacilityData()
                {
                    UniqueId = sign.id,
                    ConfigGuid = sign.configId.Guid,
                    Stats = new List<KeyValuePairSerializable<StatId, float>>(),
                    PosX = facilities[i].position.x,
                    PosZ = facilities[i].position.y,
                    Rotation = facilities[i].rotation,
                });
            }


            return f;
        }

        (string id, RuntimeId configId) GetId(CampStorageInventoryConfig inventoryConfig, ItemConfig config)
        {
            if (config.HasModule<MainContainerModule>())
                return ("MainContainer", config.Id);
            
            if (config.HasModule<CampHQModule>())
                return ("Camp HQ", config.Id);
            
            if (config.HasModule<TavernModule>())
                return ("CampTavern", config.Id);
            
            if (config.HasModule<GarageModule>())
                return ("Garage", config.Id);
            
            // if (config.HasModule<StorageModule>())
            //     return (inventoryConfig.GetInventoryId(config.Storage.StorageType), 
            //         inventoryConfig.GetConfigId(config.Storage.StorageType));
            

            Debug.LogError($"Not exist id for starting fcilities {config.Id.DebugKey}");
            return ("",null);
        }

    }

    [System.Serializable]
    public sealed class FacilityPlacementData
    {
        [Tooltip("Ссылка на FacilityModule — тип здания.")]
        public ItemConfig config;

        public Vector2Int position;
        public int rotation;
        public int level;
    }
}
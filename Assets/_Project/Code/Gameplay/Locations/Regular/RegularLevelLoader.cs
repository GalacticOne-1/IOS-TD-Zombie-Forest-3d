using UnityEngine;
using Galactic1.Repository;
using Galactic1.Configs;
using Galactic1.Core.Location;

namespace Galactic1.Gameplay.Locations.Modes
{
    /// <summary>
    /// Загрузка обычной локации — аналог LevelSetting_Regular
    /// </summary>
    public class RegularLevelLoader : ILocationLoaderMode
    {
        public void Load(LocationContext ctx, DIContainer container)
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().isPause = false;
            
            
            // устанавливаем границы ******************************************************************************
            //GameObject.Find("_GROUND_CAMP_").GetComponent<BoxCollider2D>().enabled = false;
            //var globalRepository = ServiceLocator.Current.Get<GlobalRepository>();
            
            // *** сохраняем границу локации
            // globalRepository.LocationBorderX = new Vector2(-10, ServiceLocator.Current.Get<ConfigProvider>().Get<LocationsConfigs>().Locations[globalRepository.CurrLocation].general.locationBorder.x+10);
            // globalRepository.LocationBorderY = new Vector2(-10, ServiceLocator.Current.Get<ConfigProvider>().Get<LocationsConfigs>().Locations[globalRepository.CurrLocation].general.locationBorder.y+10);
            // // применяем границу локации т.к игрок спавнится в локации только на драконе
            // new LOCATION_SETUP().SetGroundBorderX(globalRepository.LocationBorderX);
            // new LOCATION_SETUP().SetGroundBorderY(globalRepository.LocationBorderY);
            // ****************************************************************************************************
            
            // spawn for player
            //new GetPlayerSpawnPoint(out globalRepository.PlayerSpawnPoint);
            //DLog.Alert($"prefab >> {ServiceLocator.Current.Get<LibController>().mapData.Locations[globalRepository.CUR_LOCATION].general.PrefabPath}");
            
            // location
            var prefabPath = ctx.LocationConfig.PrefabPath;
            ctx.LocationInstance = prefabPath.CreateGO(ServiceLocator.Current.Get<Environment>().location.transform);

            // загрузка ящиков
            //createdLocation.GetComponent<LocationSetup>().LoadCrateItems(
            //ServiceLocator.Current.Get<LibController>().mapData.Mission[globalRepository.CUR_LOCATION].PossibleReward);

            // спавним существ
            //createdLocation.GetComponent<LocationSpawner>().LoadCreatures();
        }
    }
}
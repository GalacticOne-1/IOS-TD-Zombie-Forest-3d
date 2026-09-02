using System;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public static class EntityFactory
    {
        
        
        /// <summary>
        /// Правильное создание сущности
        /// </summary>
        /// <param name="entityConfig"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static GameObject CreateEntity<T>(T entityConfig, Vector2 position, GameObject prefab = null)
        {
            Transform folder;
            
            
            switch (entityConfig)
            {
                // case PlayerUnitConfig playerConfig:
                // {
                //     var createdEntity = $"Gameplay/Prefabs/Player/{playerConfig.PrefabPath}"
                //         .CreateGO(ServiceLocator.Current.Get<Environment>().enemies[0]).GetComponent<_PlayerUnit>();
                //     createdEntity.transform.position = position;
                //     createdEntity.UniqueId = _PointerHub.UnicEntityId;
                //     ServiceLocator.Current.Get<EntityRepository>().RegisterPlayer(createdEntity);
                //     
                //     createdEntity.name = $"Player Unit #{createdEntity.UniqueId}";
                //     
                //     LoadEntityData(entityConfig, createdEntity.gameObject);
                //     return createdEntity.gameObject;
                // }
                
                // case EnemyConfig enemyConfig:
                // {
                //     folder = ServiceLocator.Current.Get<Environment>().enemies[0];
                //     
                //     var createdEnemy = prefab == null
                //         ? $"Gameplay/Prefabs/Enemies/{enemyConfig.GetRandomPrefab()}".CreateGO(folder).GetComponent<EnemyEntity>()
                //         : prefab.CreateGO(folder).GetComponent<EnemyEntity>();
                //
                //     createdEnemy.UniqueId = Guid.NewGuid().ToString();
                //     createdEnemy.Id = _PointerHub.UnicEntityId;
                //     createdEnemy.EntityConfig = enemyConfig;
                //     createdEnemy.transform.position = position;
                //     createdEnemy.name = "Enemy "+createdEnemy.Id;
                //     createdEnemy.OnDeath += ServiceLocator.Current.Get<LevelController>().OnEnemyKilled;
                //     ServiceLocator.Current.Get<EnemyRepository>().RegisterEnemy(createdEnemy);
                //     
                //     createdEnemy.GetComponentInChildren<HealthComponentCollider>().gameObject.AddComponent<EnemySelectCollider>();
                //     
                //     // HUD
                //     var hud = "Gameplay/Prefabs/HUD/StatusBarGroupE".CreateGO(GameSetup.I.ContainerSceneHUD).GetComponent<StatusBarGroup>();
                //     hud.name = "EnemyHUD " + createdEnemy.UniqueId;
                //     createdEnemy.OnDestory += hud.gameObject.DestroyGO;
                //     createdEnemy.GetComponentInChildren<HealthComponentCollider>().CharacterHUD = hud;
                //     hud.Initialize(createdEnemy.tr);
                //     
                //     // enemy data
                //     LoadEntityData(entityConfig, createdEnemy.gameObject);
                //     return createdEnemy.gameObject;
                // }

                // case BuildConfig buildConfig:
                // {
                //     //position.x += GridController.I.GridOffset.x;
                //     //position.y += GridController.I.GridOffset.y;
                //     
                //     // var createdBuild = new CONSTRUCT_Blueprint().Create(
                //     //     buildConfig,
                //     //     position,
                //     //     out var cashLastTiles,
                //     //     out var curTiles);
                //     
                //     //var createdBuild = buildConfig.CreateObj().GetComponent<_Entity>();
                //     folder = ServiceLocator.Current.Get<Environment>().playerObj;
                //     
                //     var createdBuild = prefab == null
                //         ? $"Gameplay/Prefabs/Builds/{buildConfig.PrefabPath}".CreateGO(folder).GetComponent<BuildEntity>()
                //         : prefab.CreateGO(folder).GetComponent<BuildEntity>();
                //     
                //     createdBuild.UniqueId = Guid.NewGuid().ToString();
                //     createdBuild.Id = _PointerHub.UnicEntityId;
                //     createdBuild.EntityConfig = buildConfig;
                //     createdBuild.transform.position = position;
                //     EntityRepository.AddEntity(createdBuild);
                //     
                //     // добавляем интерфейс для модуля преграды
                //     createdBuild.GetComponentInChildren<HealthComponentCollider>().gameObject.AddComponent<EntityColliderObstacle>();
                //     createdBuild.GetComponentInChildren<HealthComponentCollider>().gameObject.AddComponent<BuildSelectCollider>();
                //
                //     // кучка мусора при уничтожении объекта
                //     createdBuild.gameObject.AddComponent<TrashComponent>();
                //     
                //     
                //     LoadEntityData(entityConfig, createdBuild.gameObject);
                //     return createdBuild.gameObject;
                // }

                
            }




            return null;
        }



        
        /// <summary>
        /// Полная активация сущности при использовании в пуле
        /// </summary>
        /// <param name="entityConfig"></param>
        /// <param name="entity">объект полученный из пула</param>
        public static void LoadDataAndActivateEntity<T>(T entityConfig, GameObject entity)
        {
            var e = entity.GetComponent<_Entity>();
            e.Entity_Reset(false);
            LoadEntityData(entityConfig, entity);
            e.Entity_Activate();
        }
        
        /// <summary>
        /// For loading state
        /// </summary>
        /// <param name="entityConfig"></param>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        public static void LoadEntityData<T>(T entityConfig, GameObject entity)
        {
            switch (entityConfig)
            {
                // case PlayerUnitConfig playerConfig:
                // {
                //     if(entity.GetComponent<PlayerEntity>())
                //     {
                //         var unit = entity.GetComponent<PlayerEntity>();
                //         
                //     }
                // } break;


                // case EnemyConfig enemyConfig:
                // {
                //     if(entity.GetComponent<EnemyEntity>())
                //     {
                //         var unit = entity.GetComponent<EnemyEntity>();
                //         unit.Entity_Initialize();
                //         unit.Entity_Initialize(new CEnemySetup()
                //         {
                //             ConfigId = enemyConfig.ConfigId,
                //             hp = enemyConfig.HpDefault,
                //             speed = enemyConfig.speed,
                //             weapon = enemyConfig.weapon,
                //             title = enemyConfig.name,
                //             //asset = asset,
                //             //levelTarget = new Vector3(-6, cashUnit.transform.position.y,0),
                //         });
                //     }
                // } break;


                // case BuildConfig buildConfig:
                // {
                //     buildConfig.GetAttribute(StatId.Health, out var attr_hp);
                //     buildConfig.GetAttribute(StatId.Armor, out var attr_armor);
                //     if (attr_hp.value == 0)
                //     {
                //         DLog.Alert($" Object {entity.name} not have HP value", EDlogColor.ORANGE);
                //         return;
                //     }
                //
                //     entity.GetComponent<BuildEntity>().Entity_Initialize();
                //     entity.GetComponent<BuildEntity>().Entity_Initialize(new CUnitData()
                //     {
                //         setup = new CSetup()
                //         {
                //             // hp
                //             hp = (short)attr_hp.value,
                //             maxHp = (short)attr_hp.value,
                //             // armor
                //             armor = (short)attr_armor.value,
                //             maxArmor = (short)attr_armor.value
                //         }
                //     });
                // } break;
            }
        }
    }
}
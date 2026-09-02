using System.Linq;
using ObservableCollections;
using R3;

namespace Galactic1
{
    /*
     *      Хранилище состояний для разных возможных карт
     *          допустим появится вторая локация где игрок сможет строить объекты
     *          для получения состояния этих баз нужно использовать >> Map[0] или Map[1] и тд
     *              Map[0].Furnitures[3]
     *              Map[0].Crates.Add()
     */
    /// <summary>
    /// Proxy >> MapState
    /// </summary>
    public class WorldDataProxy
    {
        public int Id => Origin.Id;
        public WorldData Origin { get; }

        public ObservableList<EntityProxy> Entities { get; } = new();

        public ObservableList<CrateEntityProxy> Crates { get; } = new();
        
        //public ObservableList<BuildingProxy> Facilities { get; } = new();

        
        /// <summary>
        /// Инициализации R3 для сохранения
        /// </summary>
        /// <param name="gameState">состояние игры загруженное из диска/облака и пр</param>
        public WorldDataProxy(WorldData worldData)
        {
            /*
             *      Подписываем оригинальное состояние MapState на это прокси
             *      таким образом MapState будет синхронизировано с Map
             *      т.е любое изменение в этом классе, также меняет и MapState
             *      который в свою очередь ипользуется в сервисе сохранения
             */

            Origin = worldData;
            
            
            
            // #1 construction
            // все структуры в лагере связываем с масссивом в прокси для синхроницации
            // worldData.Entities.ForEach(entityData => Entities.Add(EntitiesProxyFactory.CreateEntity(entityData)));
            //
            // // для добавления
            // Entities.ObserveAdd().Subscribe(e =>
            // {
            //     var addedEntity = e.Value;
            //     worldData.Entities.Add(addedEntity.Origin);
            // });
            //
            // // для удаления
            // Entities.ObserveRemove().Subscribe(e =>
            // {
            //     var removedEntity = e.Value;
            //     var l = worldData.Entities.Count;
            //     for (int i = 0; i < l; i++)
            //     {
            //         if(worldData.Entities[i].UniqueId == removedEntity.UniqueId)
            //             worldData.Entities.RemoveAt(i);
            //     }
            // });
            
            
            
            /*// #2 buildings
            // все структуры в лагере связываем с масссивом в прокси для синхроницации
            worldData.Facilities.ForEach(entityData => Facilities.Add(new BuildingProxy(entityData)));
           
            // для добавления
            Facilities.ObserveAdd().Subscribe(e =>
            {
                var addedEntity = e.Value;
                worldData.Facilities.Add(addedEntity.Origin as BuildingData);
                DLog.Alert($"New building {addedEntity.UniqueId}");
            });
          
            // для удаления
            Facilities.ObserveRemove().Subscribe(e =>
            {
                var removedId = e.Value.UniqueId;
                worldData.Facilities.RemoveAll(b => b.UniqueId == removedId);
                DLog.Alert($"Remove building {removedId}");
            });*/
            
            

            
            // #2 crates
            if (worldData.Crates != null)
            {
                foreach (var crate in worldData.Crates)
                    Crates.Add(new CrateEntityProxy(crate));

            
                // при добавлении нового ящика в игре, связываем его с сохранением
                Crates.ObserveAdd().Subscribe(e =>
                {
                    var newCrateEntity = e.Value;
                    worldData.Crates.Add(newCrateEntity.Origin);
                });
            
                // так же при удалении удаляем сохранение
                Crates.ObserveRemove().Subscribe(e =>
                {
                    var removedCratesEntityProxy = e.Value;
                    var removedCratesEntity = worldData.Crates.FirstOrDefault(c => c.UniqueId == removedCratesEntityProxy.Id);
                    worldData.Crates.Remove(removedCratesEntity);
                });
            }
        }

    }
}
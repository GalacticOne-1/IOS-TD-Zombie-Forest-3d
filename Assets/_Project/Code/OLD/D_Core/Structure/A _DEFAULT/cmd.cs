
namespace Galactic1
{
    public static class cmd
    {
        
        /*
         *      1 - сначало загружается локация, потом юнит
         */


        /// <summary>
        /// Восстановление текущего кнотроллера
        /// </summary>
        public static void RestoreController() {}//new UNIT_Controller().RestoreController();



        /// <summary>
        /// Create unit in camp
        /// </summary>
        public static void Player_SpawnUnitOnScene() => new PlayerUnitLoad_Camp();
        
        /// <summary>
        /// Create unit in location
        /// </summary>
        public static void Player_SpawnUnitOnSceneForLocation() => new PlayerUnitLoad_Location();
        
        /// <summary>
        /// Removes unit
        /// </summary>
        public static void Player_RemoveUnitFromScene() => new PlayerUnitUnload();







        /// <summary>
        /// Create dragon in camp
        /// </summary>
        public static void Player_SpawnDragonOnScene() => new SpawnDragon().SpawnInCamp();

        /// <summary>
        /// Create dragon in location
        /// </summary>
        public static void Player_SpawnDragonOnSceneForLocation() => new SpawnDragon().SpawnInLocation();

        /// <summary>
        /// Removes dragon
        /// </summary>
        public static void Player_RemoveDragonFromScene() => new SpawnDragon().RemoveFromScene();







        /// <summary>
        /// Загружает лоакцию со всеми зависимостями
        /// </summary>
        public static void LoadLocation() => EventBus<LoadLevelEvent>.Raise(new LoadLevelEvent());
        
        /// <summary>
        /// Очищение сцены от локации и всех ее зависимостей
        /// </summary>
        public static void ClearLocation() => EventBus<ClearLevelEvent>.Raise(new ClearLevelEvent());


        /// <summary>
        /// Загрузка всего для лагеря
        /// </summary>
        public static void LoadCampLocation()
        {
            //ServiceLocator.Current.Get<GlobalRepository>().CurrLocation = 0;
            new PlayerCamp_Loading();
        }

        /// <summary>
        /// Очищение сцены от лагеря
        /// </summary>
        public static void ClearCampLocation() => new PlayerCamp_Clear();


        
        




        #region LOCATION

        public static void LoadLocation(int id) => new LOCATION(id).Load();
        
        public static void UnloadLocation(int id) => new LOCATION(id).Unload();

        #endregion






        #region AUDIO

        

        #endregion
    }
}
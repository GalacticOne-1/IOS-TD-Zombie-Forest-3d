namespace Galactic1
{
    public class LocationEnterParams : SceneEnterParams
    {
        public int WorldStateId { get; }

        /// <summary>
        /// 3десь можно передавать разные параметры при входе на сцену
        /// </summary>
        /// <param name="worldStateId">доступ к карте состояния</param>
        public LocationEnterParams(int worldStateId) : base(Scenes.LOCATION)
        {
            WorldStateId = worldStateId;
        }
    }
}
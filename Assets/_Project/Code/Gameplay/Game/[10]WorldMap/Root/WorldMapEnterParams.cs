namespace Galactic1
{
    public class WorldMapEnterParams : SceneEnterParams
    {
        public int WorldStateId { get; }

        /// <summary>
        /// 3десь можно передавать разные параметры при входе на сцену
        /// </summary>
        /// <param name="worldStateId">доступ к карте состояния</param>
        public WorldMapEnterParams(int worldStateId) : base(Scenes.MAP)
        {
            WorldStateId = worldStateId;
        }
    }
}
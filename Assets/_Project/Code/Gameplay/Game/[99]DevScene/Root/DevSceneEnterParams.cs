namespace Galactic1
{
    public class DevSceneEnterParams : SceneEnterParams
    {
        public int WorldStateId { get; }

        /// <summary>
        /// 3десь можно передавать разные параметры при входе на сцену
        /// </summary>
        /// <param name="worldStateId">доступ к карте состояния</param>
        public DevSceneEnterParams(int worldStateId) : base(Scenes.LOCATION)
        {
            WorldStateId = worldStateId;
        }
    }
}
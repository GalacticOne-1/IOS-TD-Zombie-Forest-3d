namespace Galactic1
{
    public class CampEnterParams : SceneEnterParams
    {
        public int WorldStateId { get; }

        public bool ResetRootPlayerScene = false;

        /// <summary>
        /// 3десь можно передавать разные параметры при входе на сцену
        /// </summary>
        /// <param name="worldStateId">доступ к карте состояния</param>
        public CampEnterParams(int worldStateId) : base(Scenes.HOME)
        {
            WorldStateId = worldStateId;
        }
    }
}
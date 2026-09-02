namespace Galactic1
{
    public class WorldMapExitParams
    {
        public SceneEnterParams TargetSceneEnterParams { get; }

        public WorldMapExitParams(SceneEnterParams targetSceneEnterParams)
        {
            TargetSceneEnterParams = targetSceneEnterParams;
        }
    }
}
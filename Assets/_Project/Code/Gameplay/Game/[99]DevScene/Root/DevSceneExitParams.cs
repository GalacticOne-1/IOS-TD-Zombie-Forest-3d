namespace Galactic1
{
    public class DevSceneExitParams
    {
        public WorldMapEnterParams WorldMapEnterParams { get; }

        public DevSceneExitParams(WorldMapEnterParams worldMapEnterParams)
        {
            WorldMapEnterParams = worldMapEnterParams;
        }
        
        
    }
}
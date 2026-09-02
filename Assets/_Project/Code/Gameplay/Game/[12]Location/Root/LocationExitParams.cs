namespace Galactic1
{
    public class LocationExitParams
    {
        public WorldMapEnterParams WorldMapEnterParams { get; }

        public LocationExitParams(WorldMapEnterParams worldMapEnterParams)
        {
            WorldMapEnterParams = worldMapEnterParams;
        }
        
        
    }
}
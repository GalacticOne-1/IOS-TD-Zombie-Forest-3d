using Galactic1.Core.GameSession;

namespace Galactic1.Core.Systems.GameSession.WorldMap
{
    public static class WorldMapSessionExtensions
    {
        public static WorldMapContext WorldMapContext(this SceneSessionDefinition session)
        {
            return session.WorldMapContext;
        }
    }
}
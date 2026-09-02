using Galactic1.Gameplay.Locations.Authoring;
using UnityEngine;

namespace Galactic1.Gameplay.Locations.Navigation
{
    public readonly struct GridGraphConfigurationDTO
    {
        public readonly LocationGeometryDefinition.NavigationSettings Settings;
        public readonly Vector2 LocationSize;

        public GridGraphConfigurationDTO(
            LocationGeometryDefinition.NavigationSettings settings,
            Vector2 locationSize)
        {
            Settings = settings;
            LocationSize = locationSize;
        }
    }
}
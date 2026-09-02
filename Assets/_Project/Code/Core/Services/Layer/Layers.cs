using UnityEngine;

namespace Galactic1.Core.Gameplay
{
    public static class Layers
    {
        public static LayerMask Detectable { get; private set; }
        public static LayerMask Damageable { get; private set; }
        public static LayerMask Occlusion { get; private set; }


        public static void Setup(LayerService service)
        {
            Detectable = service.Detectable;
            Damageable = service.Damageable;
            Occlusion = service.Occlusion;
        }
    }
}
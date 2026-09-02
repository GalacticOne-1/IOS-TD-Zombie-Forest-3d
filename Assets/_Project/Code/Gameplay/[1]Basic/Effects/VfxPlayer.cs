using UnityEngine;

namespace Galactic1.PoolObject
{
    public class VfxPlayer
    {
        public GameObject Play(
            IObjectPoolConfig config, 
            EffectRequest request)
        {
            var effect = ServiceLocator.Current.Get<PoolManager>().Get<EffectPoolable>(config);
            
            if (effect == null) return null;
            
            /*
             *  - по дефолту продолжительность берется из конфига
             *  - если нужна другая, то передавать в EffectRequest.duration
             */
            if (request.Duration > 0)
                effect.Setup(request.Duration);
            
            // === меняем родителя
            if (request.AttachTo != null)
            {
                effect.AttachTo(request.AttachTo);
            }
            else
            {
                effect.transform.localPosition = request.Position;
                effect.transform.rotation = request.Rotation;
            }
            
            return effect.gameObject;
        }
    }

}